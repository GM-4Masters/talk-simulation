using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    //private List<ChatData> chatDataList;
    private List<List<ChatData>> chatDataList = new List<List<ChatData>>();
    private List<List<ChatData>> subDataList = new List<List<ChatData>>();
    private List<ChatData> tutorial = new List<ChatData>();
    private List<List<ChatData>> personalChat = new List<List<ChatData>>();

    public List<string> characterList = new List<string>(){ 
        "����", "������", "���ӽ���", "��Ʈ��", "�ý���", "����", "����",
        "�����", "���ȣ", "������", "��ä��", "����", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "������", "���ȣ", "������", "��ä��", "����", "Ʃ�丮��", "GameMasters"
    };

    public List<List<string>> noticeList = new List<List<string>>();
    public List<string> endingList = new List<string>();

    public enum DATATYPE { MAIN, SUB, PERSONAL, TUTORIAL }

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

        LoadChatData();
        LoadNoticeData("noticedata");
        LoadEndingData("endingdata");
    }

    private void LoadNoticeData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        for (int i=0; i<4; i++)
        {
            List<string> data = new List<string>();
            for(int j=0; j<3; j++)
            {
                data.Add(lines[i*3+j]);
            }
            noticeList.Add(data);
        }
    }

    private void LoadEndingData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        int endingIndex = 0;
        string endingTxt = "";
        foreach (string line in lines)
        {
            if (line.Contains("-"))
            {
                endingList.Add(endingTxt);
                endingTxt = "";
                endingIndex++;
            }
            else endingTxt+=(line+"\n");
        }
    }

    private void LoadChatData()
    {
        tutorial = CSVReader.Read("chatData_tutorial");

        for (int i = 0; i < 3; i++)
        {
            chatDataList.Add(CSVReader.Read("chatData_ep" + (i+1)));

            if (i != 2)
            {
                //������ ���� ����
                for(int j=0; j<chatDataList[i].Count; j++)
                {
                    personalChat.Add(new List<ChatData>());
                    if (chatDataList[i][j].chatroom != "������")
                    {
                        personalChat[i].Add(chatDataList[i][j]);
                    }
                }

                // ���Ǽҵ� 1,2 ������ ������ 3�� �߶�
                for (int j = 0; j < 3; j++)
                {
                    chatDataList[i].RemoveAt(chatDataList[i].Count - 1);
                }
            }

            subDataList.Add(CSVReader.Read("chatData_SubEp" + (i + 1) + "Str"));
        }

        GameManager.Instance.mainEpisodeCnt = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            GameManager.Instance.mainEpisodeCnt.Add(GetEpisodeSize(i));
        }
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
        else if (type == DATATYPE.PERSONAL) data = personalChat[episodeIndex];
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