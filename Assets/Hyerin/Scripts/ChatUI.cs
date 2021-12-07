using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;

    ChatData chatData;

    private int chatroomIndex;

    private List<ChatData> data;

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
        if (chatroomIndex == 0)
        {
            int recentIndex = GameManager.Instance.currentChatIndex;
            if (GameManager.Instance.lastChatIndex != recentIndex)
            {
                for (int i = GameManager.Instance.lastChatIndex+1; i <= recentIndex; i++)
                {
                    chatData = DataManager.Instance.GetChatData(0, i);
                    SetUI();
                }
                GameManager.Instance.lastChatIndex = recentIndex;
            }
        }
    }

    public void SetUI()
    {
        int characterIndex = DataManager.Instance.characterList.IndexOf(chatData.character);
        switch (chatData.character)
        {
            case "독백":
                Monologue();
                break;
            case "선택지":
                WaitForSelect();
                break;
            case "시스템":
                PrintSystemMsg();
                break;
            case "입장":
                ChangeReaderCount(chatData.enterNum);
                break;
            case "퇴장":
                ChangeReaderCount(-chatData.enterNum);
                break;
            case "아트님":
                Talk();
                break;
            case "게임시작":
                GameManager.Instance.ChangeWaitFlag(false);
                break;
            default:
                chatManager.Chat(false, chatData.text, chatData.time, chatData.character, "3");
                break;
        }
        //Debug.Log("채팅 갱신:" + chatData.index + "ch:" + chatData.character);
    }

    public void Monologue()
    {
        // 독백 애니메이션 끝난 후 다음으로 넘어감
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void WaitForSelect()
    {
        // 선택지 실행 후 대기(정답 위치 랜덤)
        int answerNum = Random.Range(0, 2);

        GameManager.Instance.ChangeWaitFlag(true);
    }

    public void Select(int index)
    {
        GameManager.Instance.ChangeWaitFlag(false);

        int answer = 0; // 임시
        if (answer != index) GameManager.Instance.ChangeEnding();
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
        chatManager.Chat(true, chatData.text, chatData.time, chatData.character, "3");
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void ExitChatroom()
    {
        // 튜토리얼 완료
        if (chatroomIndex == 5 && !GameManager.Instance.IsTutorialFinished()) GameManager.Instance.FinishTutorial();

        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
