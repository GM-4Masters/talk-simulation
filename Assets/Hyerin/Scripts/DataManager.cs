using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    //private List<ChatData> chatDataList;
    private List<List<ChatData>> chatDataList = new List<List<ChatData>>();
    private List<List<ChatData>> subDataList = new List<List<ChatData>>();
    private List<ChatData> tutorial = new List<ChatData>();

    public List<string> characterList = new List<string>(){ 
        "����", "������", "���ӽ���", "��Ʈ��", "�ý���", "����", "����",
        "�����", "���ȣ", "������", "��ä��", "����", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "������", "�輱ȿ", "������", "��ä��", "����", "Ʃ�丮��", "GameMasters"
    };

    public enum DATATYPE { MAIN, SUB, TUTORIAL }

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
        //chatDataList.Add(CSVReader.Read("chatdata"));
        chatDataList.Add(CSVReader.Read("chatData_ep1"));
        chatDataList.Add(CSVReader.Read("chatData_ep2"));
        chatDataList.Add(CSVReader.Read("chatData_ep3"));

        subDataList.Add(CSVReader.Read("chatData_SubEp1Str"));
        subDataList.Add(CSVReader.Read("chatData_SubEp2Str"));
        subDataList.Add(CSVReader.Read("chatData_SubEp3Str"));

        tutorial = CSVReader.Read("chatData_tutorial");
    }
    
    public int GetEpisodeSize(int episodeIndex)
    {
        return chatDataList[episodeIndex].Count;
    }

    public List<ChatData> GetChatList(int episodeIndex, DATATYPE type)
    {
        List<ChatData> data = new List<ChatData>();
        if (type == DATATYPE.MAIN) data = chatDataList[episodeIndex];
        else if (type == DATATYPE.SUB) data = subDataList[episodeIndex];
        else data = tutorial;

        return data;
    }

    public List<ChatData> GetChatList(int episodeIndex, string chatroom)
    {
        List<ChatData> data = new List<ChatData>();
        if (chatroom == "������") data = chatDataList[episodeIndex];
        else if (chatroom == "Ʃ�丮��") data = tutorial;
        else
        {
            for(int i=0; i<subDataList[episodeIndex].Count; i++)
            {
                if (subDataList[episodeIndex][i].chatroom == chatroom)
                    data.Add(subDataList[episodeIndex][i]);
            }
        }

        return data;
    }

    public ChatData GetChatData(int episodeIndex, int chatIndex)
    {
        return chatDataList[episodeIndex][chatIndex];
        //return chatDataList[chatIndex];
    }

    public ChatData GetLastTutorial()
    {
        return tutorial[tutorial.Count - 1];
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