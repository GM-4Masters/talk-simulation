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
        "독백", "선택지", "게임시작", "아트님", "시스템", "입장", "퇴장",
        "팀장님", "김산호", "벅찬우", "이채린", "엄마", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "팀단톡", "김선효", "벅찬우", "이채린", "엄마", "튜토리얼", "GameMasters"
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
        if (chatroom == "팀단톡") data = chatDataList[episodeIndex];
        else if (chatroom == "튜토리얼") data = tutorial;
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
    public int index;           // 인덱스
    public string chatroom;     // 채팅방 이름
    public float dt;            // 출력시간(이전 대화 출력 dt초 후 이 대화 출력)
    public string date;         // 보낸 날짜
    public string time;         // 보낸 시간
    public string character;    // 보낸 사람
    public string text;         // 대화내용
    public int enterNum;        // 채팅방 입/퇴장 인원수
}