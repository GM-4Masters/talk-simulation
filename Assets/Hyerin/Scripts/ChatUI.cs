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
            case "����":
                Monologue();
                break;
            case "������":
                WaitForSelect();
                break;
            case "�ý���":
                PrintSystemMsg();
                break;
            case "����":
                ChangeReaderCount(chatData.enterNum);
                break;
            case "����":
                ChangeReaderCount(chatData.enterNum);
                break;
            case "��Ʈ��":
                chatManager.Chat(true, chatData.text, chatData.time, chatData.character, "3");
                break;
            default:
                chatManager.Chat(false, chatData.text, chatData.time, chatData.character, "3");
                break;
        }
    }

    public void Monologue()
    {
        // ���� �ִϸ��̼� ���� �� �������� �Ѿ
    }

    public void WaitForSelect()
    {
        // ������ ���� �� ���
        GameManager.Instance.ChangeWaitFlag(true);
    }

    public void Select(int index)
    {
        GameManager.Instance.ChangeWaitFlag(false);

        int answer = 0; // �ӽ�
        if (answer != index) GameManager.Instance.ChangeEnding();
    }

    public void PrintSystemMsg()
    {
        // �ý��� �޽��� ���

    }

    public void ChangeReaderCount(int value)
    {
        // ����/���� ó��
    }

    public void ExitChatroom()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
