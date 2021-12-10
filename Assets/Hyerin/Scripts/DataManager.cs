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
        "독백", "선택지", "게임시작", "아트님", "시스템", "입장", "퇴장",
        "팀장님", "김산호", "벅찬우", "이채린", "엄마", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "팀단톡", "김산호", "벅찬우", "이채린", "엄마", "4MasterTalk", "GameMasters"
    };

    public List<List<string>> noticeList = new List<List<string>>();
    public List<string> endingList = new List<string>();

    // 에피소드-선택지번호
    public List<List<int[]>> goodChoiceList = new List<List<int[]>>();
    public List<List<int[]>> badChoiceList = new List<List<int[]>>();

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
        LoadChoiceData("choicedata");
    }

    public string GetPersonalChatName(int episodeNum)
    {
        return personalChat[episodeNum][0].character;
    }

    // 마지막 선택지인지 확인(배드엔딩 범위 수는 선택지수와 같음)
    public bool IsLastChoice(int episodeNum, int index)
    {
        return index==(badChoiceList[episodeNum].Count);
    }

    public bool IsLastBadEndingChat(int index)
    {
        int episodeIndex = GameManager.Instance.GetEpisodeIndex();
        return (index == badChoiceList[episodeIndex][badChoiceList[episodeIndex].Count-1][1]);
    }

    public bool IsSavePoint(int index, int episodeNum)
    {
        if (GameManager.Instance.choiceNum < 0) return false;
        return (goodChoiceList[episodeNum][GameManager.Instance.choiceNum][0] == index);
    }

    public ChatData GetLastChat(bool isGoodEnding, int episodeNum)
    {
        // 엔딩에 따라 끝지점 지정
        int last = (isGoodEnding) ?
            goodChoiceList[episodeNum][GameManager.Instance.choiceNum][1]:
            badChoiceList[episodeNum][GameManager.Instance.choiceNum][1];

        // 끝에서부터 뒤로가면서 끝지점 인덱스보다 작고, 단톡방이고, 사람인 가장 첫 데이터 리턴
        for (int i=chatDataList[episodeNum].Count-1; i>=0 ; i--)
        {
            int charIndex = characterList.IndexOf(chatDataList[episodeNum][i].character);
            if(chatDataList[episodeNum][i].index<=last &&
                chatDataList[episodeNum][i].chatroom==chatroomList[0] &&
                (charIndex>6 || charIndex == 3))
            {
                return chatDataList[episodeNum][i];
            }
        }
        return null;
    }

    private void LoadNoticeData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        for (int i=0; i<chatroomList.Count; i++)
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

    private void LoadChoiceData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        List<int[]> data;
        for(int i=0; i<3; i++)
        {
            data = new List<int[]>();
            string[] goodChoices = lines[i].Split(' ');
            for (int j=0; j<goodChoices.Length; j++)
            {
                string[] numbers = goodChoices[j].Split('~');
                data.Add(new int[] { int.Parse(numbers[0]), int.Parse(numbers[1]) });
            }
            goodChoiceList.Add(data);

            data = new List<int[]>();
            string[] badChoices = lines[i + 4].Split(' ');
            for (int j = 0; j < badChoices.Length; j++)
            {
                string[] numbers = badChoices[j].Split('~');
                data.Add(new int[] { int.Parse(numbers[0]), int.Parse(numbers[1]) });
            }
            badChoiceList.Add(data);
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
                //개인톡 따로 저장
                for(int j=0; j<chatDataList[i].Count; j++)
                {
                    personalChat.Add(new List<ChatData>());
                    if (chatDataList[i][j].chatroom != "팀단톡")
                    {
                        personalChat[i].Add(chatDataList[i][j]);
                    }
                }

                // 에피소드 1,2 마지막 개인톡 3개 잘라냄
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
    public ChatData GetFirstTutorial()
    {
        return tutorial[0];
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