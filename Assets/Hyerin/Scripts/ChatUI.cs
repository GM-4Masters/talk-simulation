using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject notification;

    ChatData chatData;

    private bool isFirstSelect = true;
    private int chatroomIndex;

    private int answerNum;

    private Image notificationBg;
    private Text notiName, notiText;

    private List<ChatData> data;

    string talkable = "팀장님,김산호,벅찬우,이채린,엄마,GameMasters,4MasterTalk";

    private IEnumerator notificationCrt;

    private void Awake()
    {
        notificationBg = notification.transform.GetComponent<Image>();
        notiName = notification.transform.GetChild(0).GetComponent<Text>();
        notiText = notification.transform.GetChild(1).GetComponent<Text>();
    }

    private void OnEnable()
    {
        chatroomIndex = DataManager.Instance.chatroomList.IndexOf(GameManager.Instance.chatroom);
        data = DataManager.Instance.GetChatList(GameManager.Instance.GetEpisodeIndex(), GameManager.Instance.chatroom);


        // 에피소드 후 갠톡
        if (GameManager.Instance.IsEpisodeFinished())
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

        GameManager.Instance.ClearReadCount();
    }

    private void Update()
    {
        // 팀 단톡일 경우
        if (chatroomIndex == 0)
        {
            int recentIndex = GameManager.Instance.currentChatIndex;
            int episodeIndex = GameManager.Instance.GetEpisodeIndex();

            // 선택지에서 세이브
            if (GameManager.Instance.selectIndex[episodeIndex].Contains(GameManager.Instance.GetChatData().index))
            {
                GameManager.Instance.Save();
            }

            // 새로 출력할 것이 생겼다면
            if (GameManager.Instance.lastChatIndex != recentIndex)
            {
                for(int i=GameManager.Instance.lastChatIndex+1; i<=recentIndex; i++)
                {
                    chatData = DataManager.Instance.GetChatData(episodeIndex, i);
                    if (chatData.chatroom != "팀단톡")
                    {
                        if (!GameManager.Instance.IsRight(chatData.index))
                        {
                            // 팀 단톡에서 개인톡을 받았을 때
                            ShowNotification();
                        }                            
                        if (recentIndex == GameManager.Instance.mainEpisodeCnt[episodeIndex] - 1)
                        {
                            StartCoroutine(WaitAndExit());
                        }
                    }
                    else
                    {
                        Play();
                        SetUI();
                    }
                    GameManager.Instance.lastChatIndex = recentIndex;
                }
            }
        }
    }

    public void Play()
    {
        if (!GameManager.Instance.IsRight(chatData.index))
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
                GameManager.Instance.ChangeWaitFlag(false);
                break;
            default:
                GameManager.Instance.ChangeWaitFlag(false);
                break;
        }
        //Debug.Log("채팅 갱신:" + chatData.index + "ch:" + chatData.character);
    }

    public void SetUI()
    {
        if (!GameManager.Instance.IsRight(chatData.index))
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
            
            chatManager.Chat(isSend, text, chatData.time, chatData.character, GameManager.Instance.groupTalkUnChecked.ToString(), image, fileName);
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

        string answer = DataManager.Instance.GetChatData(GameManager.Instance.GetEpisodeIndex(),
            GameManager.Instance.currentChatIndex - 1).text;
        string wrong = chatData.text;
        if (answerNum == 0) chatManager.SetAnswer(answer, wrong);
        else chatManager.SetAnswer(wrong, answer);
        chatManager.ShowChoice();
        isFirstSelect = true;
    }

    public void Select(int selected)
    {
        chatManager.CloseChoice();
        Debug.Log("selected:"+selected+", answer:"+answerNum);
        if (answerNum != selected) GameManager.Instance.ChangeEnding();
        GameManager.Instance.selectedNum++;
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
        if (chatroomIndex == 0) return;

        // 다음 에피소드
        if (GameManager.Instance.IsEpisodeFinished()) GameManager.Instance.GoToNextEpisode();

        // 튜토리얼 완료
        if (chatroomIndex == 5 && !GameManager.Instance.IsTutorialFinished()) GameManager.Instance.FinishTutorial();

        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }







    private IEnumerator PersonalTalkCrt()
    {
        List<ChatData> personalChat = DataManager.Instance.GetChatList(GameManager.Instance.GetEpisodeIndex(), DataManager.DATATYPE.PERSONAL);
        for(int i=0; i<personalChat.Count; i++)
        {
            bool isSend = (personalChat[i].character == "아트님");
            chatData = personalChat[i];
            SetUI();

            //if(i<3) chatManager.Chat(
            //    isSend,
            //    personalChat[i].text, chatData.time, chatData.character, GameManager.Instance.groupTalkUnChecked.ToString(), image, fileName);
        }
        yield return null;
    }

    private IEnumerator WaitAndExit()
    {
        yield return new WaitForSeconds(2f);

        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
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
        // 투명도 0(0.5초) -> 0.5(1초대기) -> (0.5초)0 으로 알림창 띄움
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            notificationBg.color = new Color(1f, 1f, 1f, time);
            notiName.color = new Color(0f, 0f, 0f, time * 2f);
            notiText.color = new Color(0f, 0f, 0f, time * 2f);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        while (time > 0f)
        {
            time -= Time.deltaTime;
            notificationBg.color = new Color(1f, 1f, 1f, time);
            notiName.color = new Color(0f, 0f, 0f, time * 2f);
            notiText.color = new Color(0f, 0f, 0f, time * 2f);
            yield return null;
        }
    }
}
