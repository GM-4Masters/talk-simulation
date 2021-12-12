using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject notice;
    [SerializeField] private GameObject popupMessage;   // 팝업메시지(입장 불가 등)
    [SerializeField] private Image fadeoutImg;

    ChatData chatData;

    private bool isGameStart = false;
    private bool isFirstSelect = true;
    private int chatroomIndex;
    private int episodeIndex;

    private int answerNum;

    private Image notificationBg;
    private Text notiName, notiText;
    private Image popupMessageImg;
    private Text popupMessageTxt;

    private List<ChatData> data;

    string talkable = "팀장님,김산호,벅찬우,이채린,엄마,GameMasters,4MasterTalk";

    private IEnumerator notificationCrt, popupEffectCrt;

    private void Awake()
    {
        notificationBg = notification.transform.GetComponent<Image>();
        notiName = notification.transform.GetChild(0).GetComponent<Text>();
        notiText = notification.transform.GetChild(1).GetComponent<Text>();

        popupMessageImg = popupMessage.GetComponent<Image>();
        popupMessageTxt = popupMessage.transform.GetChild(0).GetComponent<Text>();
    }

    private void OnEnable()
    {
        fadeoutImg.color = new Color(0, 0, 0, 0);

        episodeIndex = GameManager.Instance.GetEpisodeIndex();
        chatroomIndex = DataManager.Instance.chatroomList.IndexOf(GameManager.Instance.chatroom);
        data = DataManager.Instance.GetChatList(episodeIndex, GameManager.Instance.chatroom);

        if (episodeIndex >= 0)
        {
            // 방제 및 공지 설정
            string chatroomName = GameManager.Instance.chatroom;
            if (chatroomIndex == 0) chatroomName = "4Master팀";
            else if (GameManager.Instance.chatroom == "튜토리얼") chatroomName = "4MasterTalk";
            chatManager.SetChatScreen(chatroomName, DataManager.Instance.noticeList[chatroomIndex][episodeIndex]);


            // 현재 채팅방이 에피소드 관련 갠톡이면 에피소드 후 갠톡 진행
            if (GameManager.Instance.IsEpisodeFinished() && GameManager.Instance.chatroom.Equals(DataManager.Instance.GetPersonalChatName(episodeIndex)))
            {
                StartCoroutine(PersonalTalkCrt());
            }
            else if (chatroomIndex != 0)
            {
                // 팀단톡 이외의 톡방 세팅
                for (int i = 0; i < data.Count; i++)
                {
                    chatData = data[i];
                    SetUI();
                }

            }
        }
        else
        {
            // 방제 및 공지 설정
            string chatroomName = "4MasterTalk";
            chatManager.SetChatScreen(chatroomName, DataManager.Instance.noticeList[chatroomIndex][0]);

            // 튜토리얼 스테이지에서는 하나씩 출력
            StartCoroutine(TutorialTalkCrt());
        }



        if (chatroomIndex != 0)
        {
            // 팀단톡 아니면 읽음표시 비활성화
            List<Text> readCountTxt = GameManager.Instance.GetReadCountTxt();
            for (int i = 0; i < readCountTxt.Count; i++)
            {
                readCountTxt[i].enabled = false;
            }
        }
    }

    private void Update()
    {
        // 팀 단톡일 경우
        if (chatroomIndex == 0)
        {
            int recentIndex = GameManager.Instance.currentChatIndex;

            // 새로 출력할 것이 생겼다면
            if (GameManager.Instance.lastChatIndex != recentIndex)
            {
                for(int i=GameManager.Instance.lastChatIndex+1; i<=recentIndex; i++)
                {
                    chatData = DataManager.Instance.GetChatData(episodeIndex, i);
                    // 팀 단톡에서 개인톡을 받았을 때
                    if (chatData.chatroom != "팀단톡")
                    {
                        if (GameManager.Instance.GetEndingType()==GameManager.ENDING.NORMAL)
                        {
                            ShowNotification();
                        }
                        if ( recentIndex == GameManager.Instance.mainEpisodeCnt[episodeIndex] - 1)
                        {
                            if (GameManager.Instance.GetEndingType() == GameManager.ENDING.NORMAL)
                            {
                                StartCoroutine(WaitAndExit(GameManager.SCENE.CHATLIST, 2f));
                            }
                            else
                            {
                                StartCoroutine(WaitAndExit(GameManager.SCENE.ENDING, 2f));
                            }
                        }
                    }
                    else
                    {
                        // 마지막 에피소드 후 엔딩
                        if (recentIndex == GameManager.Instance.mainEpisodeCnt[episodeIndex] - 1)
                        {
                            float waitTime = float.Parse(chatData.text.Substring(chatData.text.Length - 3, 1));
                            StartCoroutine(WaitAndExit(GameManager.SCENE.ENDING, waitTime));
                        }
                        Play();
                        SetUI();
                    }
                    GameManager.Instance.lastChatIndex = recentIndex;
                }
            }
        }
    }

    // 출력 대상 대화인지 판단(선택지에 따라)
    private bool IsRightDialogue()
    {
        int choiceNum = GameManager.Instance.choiceNum;

        if (choiceNum < 0) return true;

        // 마지막 분기점이라면 엔딩값, 아니면 선택지 정답 여부로 범위를 지정하고
        // 현재 채팅이 해당 범위에 속하는지를 판단
        if (DataManager.Instance.IsLastDialogueSet(episodeIndex, chatData.index))
        {
            bool isGoodEnding = (GameManager.Instance.ending == GameManager.ENDING.NORMAL);
            return (DataManager.Instance.IsInRange(chatData.index, isGoodEnding, episodeIndex));
        }
        else return (DataManager.Instance.IsInRange(chatData.index, GameManager.Instance.isChoiceRight, episodeIndex));
    }

    public void Play()
    {
        if (!IsRightDialogue())
        {
            //Debug.Log("play skipped:" + chatData.index);
            GameManager.Instance.SkipTime();
            GameManager.Instance.ChangeWaitFlag(false);
            return;
        };

        switch (chatData.character)
        {
            case "독백":
                Monologue();
                break;
            case "선택지":
                WaitForSelect();
                break;
            case "게임시작":
                isGameStart = true;
                GameManager.Instance.ChangeWaitFlag(false);
                break;
            default:
                GameManager.Instance.ChangeWaitFlag(false);
                break;
        }

        if (isGameStart && (talkable.Contains(chatData.character) || chatData.character == "아트님"))
        {
            GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.POP);
        }
    }

    public void SetUI()
    {
        if (!IsRightDialogue())
        {
            //Debug.Log("setui skipped:" + chatData.index);
            return;
        };
        if (talkable.Contains(chatData.character) || chatData.character=="아트님")
        {
            bool isSend = (chatData.character=="아트님");
            string text = "";
            string image = "";
            string fileName = "";
            if (chatData.text.Contains("image")) image = chatData.text;
            else if (chatData.text.Contains("+")) fileName = chatData.text.Substring(1);
            else text = chatData.text;

            // 단톡방이고 안읽은숫자가 0이 아니면 읽은숫자 표시
            string numStr = "";
            if (GameManager.Instance.chatroom == DataManager.Instance.chatroomList[0] &&
                GameManager.Instance.groupTalkUnChecked != 0)
            {
                numStr = GameManager.Instance.groupTalkUnChecked.ToString();
            }
            chatManager.Chat(isSend, text, chatData.time, chatData.character, numStr, image, fileName);
        }

        switch (chatData.character)
        {
            case "시스템":
                chatManager.SetDate(chatData.text);
                break;
            case "입장":
                ChangeReaderCount(-chatData.enterNum);
                break;
            case "퇴장":
                ChangeReaderCount(chatData.enterNum);
                break;
            default:
                break;
        }
    }

    public void Monologue()
    {
        // 독백 애니메이션 끝난 후 다음으로 넘어감
        float waitTime = float.Parse(chatData.text.Substring(chatData.text.Length - 3, 1));
        chatManager.PrintAside(chatData.text.Substring(0, chatData.text.Length - 4), waitTime);
        StartCoroutine(MonologueCrt(waitTime * 3));
    }

    private IEnumerator MonologueCrt(float second)
    {
        yield return new WaitForSeconds(second);
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void WaitForSelect()
    {
        if (isFirstSelect)
        {
            isFirstSelect = false;
            GameManager.Instance.GoNext();
            return;
        }
        // 선택지 실행 후 대기(정답 위치 랜덤)
        GameManager.Instance.ChangeWaitFlag(true);
        int answerNum = Random.Range(0, 2);
        this.answerNum = answerNum;

        string answer = DataManager.Instance.GetChatData(episodeIndex,
            GameManager.Instance.currentChatIndex - 1).text;
        string wrong = chatData.text;
        if (answerNum == 0) chatManager.SetAnswer(answer, wrong);
        else chatManager.SetAnswer(wrong, answer);
        isFirstSelect = true;
    }

    public void Select(int selected)
    {
        //GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.)
        chatManager.CloseChoice();
        //Debug.Log("selected:"+selected+", answer:"+answerNum);
        GameManager.Instance.isChoiceRight = (answerNum == selected);
        if (answerNum != selected) GameManager.Instance.ChangeEnding();
        GameManager.Instance.choiceNum++;
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void ChangeReaderCount(int value)
    {
        GameManager.Instance.groupTalkUnChecked += value;
        List<Text> readCountTxt = GameManager.Instance.GetReadCountTxt();
        for(int i=0; i<readCountTxt.Count; i++)
        {
            string numStr = (GameManager.Instance.groupTalkUnChecked == 0) ? 
                "" : GameManager.Instance.groupTalkUnChecked.ToString();
            if (readCountTxt[i].text!="" && int.Parse(readCountTxt[i].text)> GameManager.Instance.groupTalkUnChecked)
                readCountTxt[i].text = numStr;
        }
    }

    public void ExitChatroom()
    {
        //isDisplayed = false;
        if (chatroomIndex == 0)
        {
            // 에피소드 진행 중에는 나갈 수 없음
            if (popupEffectCrt != null) StopCoroutine(popupEffectCrt);
            popupEffectCrt = PopupEffectCrt();
            StartCoroutine(popupEffectCrt);
            return;
        }

        // 갠톡 종료 후 다음 에피소드
        if (GameManager.Instance.IsEpisodeFinished() &&
            GameManager.Instance.chatroom.Equals(DataManager.Instance.GetPersonalChatName(episodeIndex))) GameManager.Instance.GoToNextEpisode();

        // 튜토리얼 완료(다음 에피소드)
        if (chatroomIndex == 5 && !GameManager.Instance.IsTutorialFinished()) GameManager.Instance.FinishTutorial();

        // 일반 채팅방 나가기
        //GameManager.Instance.Save();
        GameManager.Instance.ClearReadCount();
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }

    private IEnumerator PopupEffectCrt()
    {
        popupMessage.SetActive(true);

        float time = 0f;
        Color bgColor = popupMessageImg.color;  // 0f ~ 0.5f
        Color txtColor = popupMessageTxt.color; // 0f ~ 1f
        while (time < 2f)
        {
            time += Time.deltaTime;
            bgColor.a = Mathf.Clamp((Mathf.Sin(time * 2) * 2), 0, 1) * 0.5f;
            txtColor.a = Mathf.Clamp((Mathf.Sin(time * 2) * 2), 0, 1);
            popupMessageImg.color = bgColor;
            popupMessageTxt.color = txtColor;
            yield return null;
        }

        popupMessage.SetActive(false);
    }


    private IEnumerator TutorialTalkCrt()
    {
        List<ChatData> tutorialChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.TUTORIAL);
        for (int i = 0; i < tutorialChat.Count; i++)
        {
            GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.POP);
            chatData = tutorialChat[i];
            SetUI();
            yield return new WaitForSeconds(2f);
        }
        yield return null;
    }

    private IEnumerator PersonalTalkCrt()
    {
        List<ChatData> personalChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
        for(int i=0; i<personalChat.Count; i++)
        {
            bool isSend = (personalChat[i].character == "아트님");
            if (i > 2)
            {
                yield return new WaitForSeconds(chatData.dt);
                GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.POP);
            }
            chatData = personalChat[i];
            SetUI();

            //if(i<3) chatManager.Chat(
            //    isSend,
            //    personalChat[i].text, chatData.time, chatData.character, GameManager.Instance.groupTalkUnChecked.ToString(), image, fileName);
        }
        yield return null;
    }

    private IEnumerator WaitAndExit(GameManager.SCENE scene, float waitTime)
    {
        Debug.Log("waitTime:" + waitTime);
        yield return new WaitForSeconds(waitTime);

        // 엔딩으로 가기 전이면 페이드아웃
        if (scene == GameManager.SCENE.ENDING)
        {
            float time = 0f;
            Color color = fadeoutImg.color;
            while (time < 2f)
            {
                time += (Time.deltaTime * GameManager.Instance.GetPlaySpeed());
                color.a = time * (1 / 2f);
                fadeoutImg.color = color;
                yield return null;
            }
        }
        fadeoutImg.color = new Color(0, 0, 0, 1);

        GameManager.Instance.ClearReadCount();
        GameManager.Instance.ChangeScene(scene);
    }

    // 알림 
    public void ShowNotification()
    {
        notiName.text = chatData.character;
        notiText.text = chatData.text;

        if (notificationCrt != null) StopCoroutine(notificationCrt);
        notificationCrt = NotificationCrt();
        StartCoroutine(notificationCrt);
    }

    private IEnumerator NotificationCrt()
    {
        GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.ALERT);

        // 투명도 0(0.5초) -> 0.7(1초대기) -> (0.5초)0 으로 알림창 띄움
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            notificationBg.color = new Color(1f, 1f, 1f, time*1.4f);
            notiName.color = new Color(0f, 0f, 0f, time * 2f);
            notiText.color = new Color(0f, 0f, 0f, time * 2f);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        while (time > 0f)
        {
            time -= Time.deltaTime;
            notificationBg.color = new Color(1f, 1f, 1f, time*1.4f);
            notiName.color = new Color(0f, 0f, 0f, time * 2f);
            notiText.color = new Color(0f, 0f, 0f, time * 2f);
            yield return null;
        }
        notificationBg.color = new Color(1f, 1f, 1f, 0f);
        notiName.color = new Color(0f, 0f, 0f, 0f);
        notiText.color = new Color(0f, 0f, 0f, 0f);
    }
}
