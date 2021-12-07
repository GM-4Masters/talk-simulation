using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;

    ChatData chatData;

    private void Awake()
    {
    }

    private void Update()
    {
        int recentIndex = GameManager.Instance.currentChatIndex;
        if(GameManager.Instance.lastChatIndex != recentIndex)
        {
            for(int i= GameManager.Instance.lastChatIndex + 1; i< recentIndex; i++)
            {
                chatData = DataManager.Instance.GetChatData(0, i);
                Play();
            }
            GameManager.Instance.lastChatIndex = recentIndex;
        }
    }

    public void Play()
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
                ChangeReaderCount(chatData.enterNum);
                break;
            case "아트님":
                chatManager.Chat(true, chatData.text, chatData.time, chatData.character, "3");
                break;
            default:
                chatManager.Chat(false, chatData.text, chatData.time, chatData.character, "3");
                break;
        }
    }

    public void Monologue()
    {
        // 독백 애니메이션 끝난 후 다음으로 넘어감
    }

    public void WaitForSelect()
    {
        // 선택지 실행 후 대기
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

    public void ExitChatroom()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
