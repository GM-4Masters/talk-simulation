using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    private List<ChatData> chatDataList;

    public List<string> characterList = new List<string>(){ 
        "����", "������", "���ӽ���", "��Ʈ��", "�ý���", "����", "����",
        "�����", "���ȣ", "������", "��ä��", "����", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "������", "�輱ȿ", "������", "��ä��", "����", "Ʃ�丮��", "GameMasters"
    };

    public static DataManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(DataManager)) as DataManager;

                if (instance == null)
                    Debug.Log("no Singleton obj");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        LoadData();
    }

    private void LoadData()
    {
        chatDataList = CSVReader.Read("chatdata");
    }

    public ChatData GetChatData(int episodeIndex, int chatIndex)
    {
        return chatDataList[chatIndex];
    }
}

public class ChatData
{
    public int index;           // �ε���
    public string chatroom;     // ä�ù� �̸�
    public float dt;            // ��½ð�(���� ��ȭ ��� dt�� �� �� ��ȭ ���)
    public string date;         // ���� ��¥
    public string time;         // ���� �ð�
    public string character;    // ���� ���
    public string text;         // ��ȭ����
    public int enterNum;        // ä�ù� ��/���� �ο���
}