using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject notice;
    [SerializeField] private GameObject popupMessage;   // 팝업메시지(입장 불가 등)
    [SerializeField] private Image fadeoutImg;

    private PlayableDirector notificationPd;
    private PlayableDirector popupMessagePd;

    private bool isWait = false;        // 채팅 대기 상태
    private float spendTime = 0f;

    private bool isFirstSelect = true;  // 각 선택지의 첫번째 답인지
    private int episodeIndex;
    private int lastChatIndex;
    private int currentChatIndex;
    private int passedChoiceNum = -1;   // 지나친 선택지 개수(-1부터 시작)
    private int groupTalkUnchecked = 3; // 안읽은 사람 수(단톡방 한정)
    private string chatroom;

    private int answerNum;

    ChatData currentChatData;

    private Text notiName, notiText;
    private Image notiImg;

    private List<ChatData> chatList;
    private List<Text> readCountTxt;
    private List<ChatData> personalChatList;
    private WaitForSeconds wait;

    private Color fadeoutColor;
    private Color black = new Color(0, 0, 0, 1);

    private string chatText,chatImagePath,chatFileName,readCountStr;
    private string answerStr, wrongStr;

    private void Awake()
    {
        notificationPd = notification.GetComponent<PlayableDirector>();
        notiName = notification.transform.GetChild(0).GetComponent<Text>();
        notiText = notification.transform.GetChild(1).GetComponent<Text>();
        notiImg = notification.transform.GetChild(2).GetComponent<Image>();

        popupMessagePd = popupMessage.GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        GameManager.Instance.ClearReadCount();

        fadeoutImg.color = new Color(0, 0, 0, 0);

        episodeIndex = GameManager.Instance.EpisodeIndex;
        chatroom = GameManager.Instance.Chatroom;
        chatList = DataManager.Instance.GetChatList(episodeIndex, chatroom);

        // 방제 및 공지 설정
        int index = episodeIndex;
        string chatroomName = chatroom;
        if (chatroom.Equals(Constants.grouptalk)) chatroomName = Constants.grouptalkName;
        if (chatroom.Equals(Constants.mastertalk)) index = 0;
        chatManager.SetChatScreen(chatroomName, DataManager.Instance.noticeList[chatroom][index]);

        // 그룹채팅방 아니면 읽음표시 비활성화
        if (!GameManager.Instance.Chatroom.Equals(Constants.grouptalk))
        {
            readCountTxt = GameManager.Instance.ReadCountTxt;
            for (int i = 0; i < readCountTxt.Count; i++)
            {
                readCountTxt[i].enabled = false;
            }
        }

        // 채팅방 유형에 따라 대기없이 바로 출력할 범위를 지정 
        if (chatroom.Equals(Constants.grouptalk))
        {
            // 그룹채팅방
            lastChatIndex = GameManager.Instance.CurrentChatIndex;
        }
        else if (chatroom.Equals(Constants.mastertalk))
        {
            // 튜토리얼
            lastChatIndex = ((GameManager.Instance.IsTutorialFinished)? chatList.Count - 1 : 0);
        }
        else if (GameManager.Instance.IsEpisodeFinished &&
                chatroom.ToString().Equals(DataManager.Instance.GetPersonalChatName(episodeIndex)))
        {
            // 에피소드 후 개인채팅방
            chatList = DataManager.Instance.GetChatList(episodeIndex,DataManager.DATATYPE.PERSONAL);
            lastChatIndex = chatList.Count - 4;   //이거맞나
        }
        else
        {
            // 서브채팅방(한번에 출력)
            lastChatIndex = chatList.Count - 1;
        }


        StartCoroutine(ChatCrt());
    }

    private IEnumerator ChatCrt()
    {
        currentChatIndex = lastChatIndex;

        // 대기 없이 바로 출력해야 하는 앞부분 채팅
        for (int i=0; i<=currentChatIndex; i++)
        {
            currentChatData = chatList[i];

            if (chatroom.Equals(Constants.grouptalk))
            {
                if (DataManager.Instance.goodChoiceList[episodeIndex][passedChoiceNum + 1][0] == currentChatData.index ||
                    DataManager.Instance.badChoiceList[episodeIndex][passedChoiceNum + 1][0] == currentChatData.index)
                    passedChoiceNum++;
            }

            if (IsRightDialogue())
            {
                SetUI(i);
            }
        }
        GoNext();

        //Debug.Log(currentChatIndex+", last:"+(chatList.Count-1));
        // 모든 채팅 출력이 끝날때까지 진행
        while (currentChatIndex < chatList.Count)
        {
            if (!isWait)
            {
                if (IsRightDialogue())
                {
                    // 대기 중
                    if (spendTime < currentChatData.dt)
                    {
                        spendTime += (Time.deltaTime * GameManager.Instance.Speed);
                    }
                    else
                    {
                        // 출력

                        switch (currentChatData.character)
                        {
                            case Constants.monologue:
                                // 독백 애니메이션 끝난 후 다음으로 넘어감
                                float waitTime = float.Parse(currentChatData.text.Substring(currentChatData.text.Length - 3, 1));
                                chatManager.PrintAside(currentChatData.text.Substring(0, currentChatData.text.Length - 4), waitTime);
                                yield return new WaitForSeconds(waitTime * 3);
                                isWait = false;
                                break;
                            case Constants.choice:
                                WaitForSelect();
                                break;
                            case Constants.gameStart:
                                isWait = false;
                                break;
                            default:
                                isWait = false;
                                break;
                        }
                        // 대화형 채팅에서만 효과음 재생
                        if (Constants.talkable.Contains(currentChatData.character))
                            GameManager.Instance.Audio.PlayEffect(AudioController.EFFECT.POP);

                        SetUI(currentChatIndex);
                        GoNext();
                    }
                }
                else
                {
                    GoNext();
                }
            }

            yield return null;
        }

        // 그룹채팅방에서 모든 채팅 출력이 끝났을 때
        if (chatroom.Equals(Constants.grouptalk) && !GameManager.Instance.IsEpisodeFinished)
        {
            wait = new WaitForSeconds(2f);

            if (episodeIndex!=2 && GameManager.Instance.Ending == GameManager.ENDING.NORMAL)
            {
                // 마지막 에피소드가 아니고 배드엔딩도 아니라면 개인메시지 팝업 표시
                personalChatList = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
                for (int i=1; i < 3; i++)
                {
                    yield return wait;
                    notificationPd.Stop();
                    //// 플레이어 메시지 이전까지만 뜨도록
                    //if (personalChatList[i].character.Equals(Constants.me)) break;

                    notiName.text = personalChatList[i].character;
                    notiText.text = personalChatList[i].text;
                    notiImg.sprite = Resources.Load<Sprite>("Sprites/Profile/" + personalChatList[i].character);

                    // 코루틴 대신 애니메이터를 키자
                    notificationPd.Play();
                    GameManager.Instance.Audio.PlayEffect(AudioController.EFFECT.ALERT);
                }

                // 마지막 메시지 세이브
                GameManager.Instance.CurrentChatIndex = DataManager.Instance.GetLastMainChat(episodeIndex);
                GameManager.Instance.IsEpisodeFinished = true;
                GameManager.Instance.Save();

                // 2초 대기 후 채팅방 자동으로 나가기
                yield return wait;
                //GameManager.Instance.ClearReadCount();
                GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
            }
            else
            {
                // 그렇지 않으면 페이드아웃 후 엔딩
                yield return wait;

                // 페이드아웃
                float time = 0f;
                fadeoutColor = fadeoutImg.color;
                while (time < 2f)
                {
                    time += (Time.deltaTime * GameManager.Instance.Speed);
                    fadeoutColor.a = time * 0.5f;
                    fadeoutImg.color = fadeoutColor;
                    yield return null;
                }
                fadeoutImg.color = black;

                //GameManager.Instance.ClearReadCount();
                GameManager.Instance.ChangeScene(GameManager.SCENE.ENDING);
            }
        }
    }

    private void GoNext()
    {
        currentChatIndex++;
        if(currentChatIndex<chatList.Count) currentChatData = chatList[currentChatIndex];
        spendTime = 0f;
    }

    // 출력 대상 대화인지 판단(선택지에 따라)
    private bool IsRightDialogue()
    {
        if (passedChoiceNum < 0) return true;


        // 마지막 분기점이라면 엔딩값, 아니면 선택지 정답 여부로 범위를 지정하고
        // 현재 채팅이 해당 범위에 속하는지를 판단
        if (DataManager.Instance.IsLastDialogueSet(episodeIndex, currentChatData.index))
        {
            bool isGoodEnding = (GameManager.Instance.Ending == GameManager.ENDING.NORMAL);
            return (DataManager.Instance.IsInRange(currentChatData.index, isGoodEnding, episodeIndex, passedChoiceNum+1));
        }
        else
        {
            bool myChoice = GameManager.Instance.IsChoiceRight[passedChoiceNum];
            return (DataManager.Instance.IsInRange(currentChatData.index, myChoice, episodeIndex, passedChoiceNum));
        }
    }

    public void SetUI(int index)
    {
        if (Constants.talkable.Contains(currentChatData.character))
        {
            bool isSend = (currentChatData.character.Equals(Constants.me));
            chatText = "";
            chatImagePath = "";
            chatFileName = "";
            if (currentChatData.text.Contains("image")) chatImagePath = currentChatData.text;
            else if (currentChatData.text.Contains("+")) chatFileName = currentChatData.text.Substring(1);
            else chatText = currentChatData.text;

            // 단톡방이고 안읽은숫자가 0이 아니면 읽은숫자 표시
            readCountStr = "";
            if (GameManager.Instance.Chatroom.Equals(Constants.grouptalk) &&
                groupTalkUnchecked != 0)
            {
                readCountStr = groupTalkUnchecked.ToString();
            }
            chatManager.Chat(isSend, chatText, currentChatData.time, currentChatData.character, readCountStr, chatImagePath, chatFileName);
        }

        switch (currentChatData.character)
        {
            case "시스템":
                chatManager.SetDate(currentChatData.text);
                break;
            case "입장":
                ChangeReaderCount(-currentChatData.enterNum);
                break;
            case "퇴장":
                ChangeReaderCount(currentChatData.enterNum);
                break;
            default:
                break;
        }
    }

    public void WaitForSelect()
    {
        if (isFirstSelect)
        {
            isFirstSelect = false;
            isWait = false;
            return;
        }
        // 선택지 실행 후 대기(정답 위치 랜덤)
        isWait=true;
        int answerNum = Random.Range(0, 2);
        this.answerNum = answerNum;

        answerStr = chatList[currentChatIndex-1].text;
        wrongStr = currentChatData.text;
        if (answerNum == 0) chatManager.SetAnswer(answerStr, wrongStr);
        else chatManager.SetAnswer(wrongStr, answerStr);
        isFirstSelect = true;
    }

    // 선택지 선택
    public void Select(int selected)
    {
        chatManager.CloseChoice();
        //Debug.Log("selected:"+selected+", answer:"+answerNum);
        GameManager.Instance.AddChoiceResult(answerNum == selected);
        if (answerNum != selected) GameManager.Instance.ChangeEnding();
        passedChoiceNum++;
        GameManager.Instance.ChoiceNum++;

        if (answerNum == selected)
        {
            GameManager.Instance.CurrentChatIndex = currentChatIndex;
        }
        else
        {
            GameManager.Instance.CurrentChatIndex = currentChatIndex + DataManager.Instance.GetBadEndingOffset(episodeIndex, passedChoiceNum);
        }
        GameManager.Instance.Save();
        spendTime += 1000f;
        isWait=false;
    }

    public void ChangeReaderCount(int value)
    {
        groupTalkUnchecked += value;
        readCountTxt = GameManager.Instance.ReadCountTxt;
        for(int i=0; i<readCountTxt.Count; i++)
        {
            readCountStr = (groupTalkUnchecked == 0) ? 
                "" : groupTalkUnchecked.ToString();
            if (readCountTxt[i].text!="" && int.Parse(readCountTxt[i].text)> groupTalkUnchecked)
                readCountTxt[i].text = readCountStr;
        }
    }



    public void ExitChatroom()
    {
        // 그룹채팅방 - 나갈 수 없음 (팝업 띄우기)
        if (GameManager.Instance.Chatroom.Equals(Constants.grouptalk) && !GameManager.Instance.IsEpisodeFinished)
        {
            popupMessagePd.Play();
            return;
        }

        // 개인채팅방 - 종료 후 다음 에피소드로 이동
        if (GameManager.Instance.IsEpisodeFinished &&
            GameManager.Instance.Chatroom.ToString().Equals(DataManager.Instance.GetPersonalChatName(episodeIndex)))
            GameManager.Instance.GoToNextEpisode();

        // 튜토리얼채팅방(최초 튜토리얼 완료) - 첫번째 에피소드로 이동
        if (GameManager.Instance.Chatroom.Equals(Constants.mastertalk) &&
            !GameManager.Instance.IsTutorialFinished)
            GameManager.Instance.FinishTutorial();

        // 일반 나가기(서브채팅방, 에피소드 종료된 그룹채팅방, 튜토리얼 완료된 튜토리얼채팅방)
        //GameManager.Instance.ClearReadCount();
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
