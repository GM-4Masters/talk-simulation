using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    private List<ChatData> chatDataList;

    public List<string> characterList = new List<string>(){ 
        "독백", "선택지", "게임시작", "아트님", "시스템", "입장", "퇴장",
        "팀장님", "김산호", "벅찬우", "이채린", "엄마", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "팀단톡", "김선효", "벅찬우", "이채린", "엄마", "튜토리얼", "GameMasters"
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
    public int index;           // 인덱스
    public string chatroom;     // 채팅방 이름
    public float dt;            // 출력시간(이전 대화 출력 dt초 후 이 대화 출력)
    public string date;         // 보낸 날짜
    public string time;         // 보낸 시간
    public string character;    // 보낸 사람
    public string text;         // 대화내용
    public int enterNum;        // 채팅방 입/퇴장 인원수
}