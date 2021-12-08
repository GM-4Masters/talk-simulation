using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;

    ChatData chatData;

    private bool isFirstSelect = true;
    private int chatroomIndex;

    private int answer;

    private List<ChatData> data;

    string talkable = "팀장님,김산호,벅찬우,이채린,엄마,GameMasters,4MasterTalk";

    private void OnEnable()
    {
        chatroomIndex = DataManager.Instance.chatroomList.IndexOf(GameManager.Instance.chatroom);
        data = DataManager.Instance.GetChatList(GameManager.Instance.GetEpisodeIndex(), GameManager.Instance.chatroom);

        // 팀단톡이라면 마지막으로 갱신했던 인덱스까지 출력
        int endIndex = ((chatroomIndex != 0) ? data.Count : GameManager.Instance.lastChatIndex);

        for (int i = 0; i < endIndex; i++)
        {
            chatData = data[i];
            SetUI();
        }
    }

    private void Update()
    {
        // 팀 단톡일 경우
        if (chatroomIndex == 0 && !GameManager.Instance.IsSkipping())
        {
            int recentIndex = GameManager.Instance.currentChatIndex;
            if (GameManager.Instance.lastChatIndex != recentIndex)
            {
                for (int i = GameManager.Instance.lastChatIndex+1; i <= recentIndex; i++)
                {
                    chatData = DataManager.Instance.GetChatData(0, i);
                    Play();
                    SetUI();
                }
                GameManager.Instance.lastChatIndex = recentIndex;
            }
        }
    }

    public void Play()
    {
        if (!GameManager.Instance.IsRight(chatData.index))
        {
            Debug.Log("play skipped:"+chatData.index);
            return;
        };
        int characterIndex = DataManager.Instance.characterList.IndexOf(chatData.character);

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
            Debug.Log("setui skipped:" + chatData.index);
            return;
        };
        if (talkable.Contains(chatData.character)) chatManager.Chat(false, chatData.text, chatData.time, chatData.character, "3");
        else if(chatData.character=="아트님") chatManager.Chat(true, chatData.text, chatData.time, chatData.character, "3");

        switch (chatData.character)
        {
            case "시스템":
                PrintSystemMsg();
                break;
            case "입장":
                ChangeReaderCount(chatData.enterNum);
                break;
            case "퇴장":
                ChangeReaderCount(-chatData.enterNum);
                break;
            default:
                break;
        }
    }

    public void Monologue()
    {
        // 독백 애니메이션 끝난 후 다음으로 넘어감
        chatManager.PrintAside(chatData.text.Substring(0,chatData.text.Length-4));
        StartCoroutine(MonologueCrt(chatData.dt * 2));
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
        string wrong = chatData.text;
        string answer = DataManager.Instance.GetChatData(GameManager.Instance.GetEpisodeIndex(),
            GameManager.Instance.currentChatIndex-1).text;
        GameManager.Instance.ChangeWaitFlag(true);
        int answerNum = Random.Range(0, 2);
        this.answer = answerNum;
        if (answerNum == 0) chatManager.SetAnswer(answer, wrong);
        else chatManager.SetAnswer(wrong, answer);
        chatManager.ShowChoice();
        isFirstSelect = true;
    }

    public void Select(int selected)
    {
        chatManager.CloseChoice();
        Debug.Log("answer:"+answer+", selected:"+selected);
        if (answer != selected) GameManager.Instance.ChangeEnding();
        GameManager.Instance.selectedNum++;
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void PrintSystemMsg()
    {
        // 시스템 메시지 출력

    }

    public void ChangeReaderCount(int value)
    {
        // 입장/퇴장 처리
    }

    public void Talk()
    {
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void ExitChatroom()
    {
        // 튜토리얼 완료
        if (chatroomIndex == 5 && !GameManager.Instance.IsTutorialFinished()) GameManager.Instance.FinishTutorial();

        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
