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

        // �������̶�� ���������� �����ߴ� �ε������� ���
        int endIndex = ((chatroomIndex != 0) ? data.Count : GameManager.Instance.lastChatIndex);

        for (int i = 0; i < endIndex; i++)
        {
            chatData = data[i];
            SetUI();
        }
    }

    private void Update()
    {
        // �� ������ ���
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
                ChangeReaderCount(-chatData.enterNum);
                break;
            case "��Ʈ��":
                Talk();
                break;
            case "���ӽ���":
                GameManager.Instance.ChangeWaitFlag(false);
                break;
            default:
                chatManager.Chat(false, chatData.text, chatData.time, chatData.character, "3");
                break;
        }
        //Debug.Log("ä�� ����:" + chatData.index + "ch:" + chatData.character);
    }

    public void Monologue()
    {
        // ���� �ִϸ��̼� ���� �� �������� �Ѿ
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void WaitForSelect()
    {
        // ������ ���� �� ���(���� ��ġ ����)
        int answerNum = Random.Range(0, 2);

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

    public void Talk()
    {
        chatManager.Chat(true, chatData.text, chatData.time, chatData.character, "3");
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void ExitChatroom()
    {
        // Ʃ�丮�� �Ϸ�
        if (chatroomIndex == 5 && !GameManager.Instance.IsTutorialFinished()) GameManager.Instance.FinishTutorial();

        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
