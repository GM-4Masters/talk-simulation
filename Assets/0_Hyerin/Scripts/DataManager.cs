using System.Collections.Generic;
using UnityEngine;

static class Constants
{
    public static readonly string grouptalk = "팀단톡";
    public static readonly string mastertalk = "4MasterTalk";

    public static readonly string grouptalkName = "4Master팀";

    public static readonly string talkable = "아트님,팀장님(기획),김산호(플밍),벅찬우,이채린(플밍),엄마,GameMasters,4MasterTalk";
    public const string me = "아트님";


    public const string monologue = "독백";
    public const string choice = "선택지";
    public const string gameStart = "게임시작";
}

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    //private List<ChatData> chatDataList;
    private List<List<ChatData>> mainChatList = new List<List<ChatData>>();
    private List<List<List<ChatData>>> subChatList = new List<List<List<ChatData>>>();
    private List<ChatData> tutorialChat = new List<ChatData>();
    private List<List<ChatData>> personalChatList = new List<List<ChatData>>();

    public List<string> characterList = new List<string>(){ 
        "독백", "선택지", "게임시작", "아트님", "시스템", "입장", "퇴장",
        "팀장님(기획)", "김산호(플밍)", "벅찬우", "이채린(플밍)", "엄마", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "팀단톡", "김산호(플밍)", "벅찬우", "이채린(플밍)", "엄마", "4MasterTalk", "GameMasters"
    };

    public Dictionary<string, List<string>> noticeList = new Dictionary<string, List<string>>();
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

    private void OnDestroy()
    {
        instance = null;
        Resources.UnloadUnusedAssets();
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

    public List<ChatData> GetChatList(int episodeIndex, DATATYPE type)
    {
        List<ChatData> data = new List<ChatData>();
        if (type == DATATYPE.MAIN) data = mainChatList[episodeIndex];
        else if (type == DATATYPE.PERSONAL) data = personalChatList[episodeIndex];
        else data = tutorialChat;

        return data;
    }

    public List<ChatData> GetChatList(int episodeIndex, string chatroom)
    {
        List<ChatData> data = new List<ChatData>();
        if (chatroom == Constants.grouptalk) data = mainChatList[episodeIndex];
        else if (chatroom == Constants.mastertalk) data = tutorialChat;
        else
        {
            for (int i = 0; i < subChatList[episodeIndex].Count; i++)
            {
                if (subChatList[episodeIndex][i][0].chatroom.Equals(chatroom))
                    data = subChatList[episodeIndex][i];
            }
        }
        // 이 함수에서 개인채팅방 데이터는 반환할 수 없음(DATATYPE을 이용해서 받아와야 함)

        return data;
    }

    public ChatData GetMainChat(int episodeIndex, int chatIndex)
    {
        return mainChatList[episodeIndex][chatIndex];
        //return chatDataList[chatIndex];
    }

    // 해당 에피소드의 가장 마지막 대화 가져오기(굿엔딩 에피소드 종료 후 저장 지점. 마지막 에피소드에서는 보여지지 않음)
    public int GetLastMainChat(int episodeNum)
    {
        int index = 0;
        for(int i=mainChatList[episodeNum].Count-1; i>=0; i--)
        {
            if (mainChatList[episodeNum][i].index == goodChoiceList[episodeNum][goodChoiceList[episodeNum].Count - 2][1]) index = i;
        }
        return index;
    }

    public List<List<ChatData>> GetAllSubChat(int episodeNum)
    {
        return subChatList[episodeNum];
    }

    // 해당 에피소드의 서브채팅방의 마지막 채팅만을 반환
    public List<ChatData> GetLastSubChat(int episodeNum)
    {
        List<ChatData> result = new List<ChatData>();
        for(int i=0; i<subChatList[episodeNum].Count; i++)
        {
            result.Add(subChatList[episodeNum][i][subChatList[episodeNum][i].Count - 1]);
        }
        return result;
    }

    public ChatData GetFirstTutorial()
    {
        return tutorialChat[0];
    }

    public ChatData GetLastTutorial()
    {
        return tutorialChat[tutorialChat.Count - 1];
    }

    public string GetPersonalChatName(int episodeNum)
    {
        return personalChatList[episodeNum][0].chatroom;
    }

    // 에피소드 전환 시 시작지점(게임시작 직전)
    public int GetStartIndex(int episodeNum)
    {
        int index = 0;
        while (!mainChatList[episodeNum][index].character.Equals(Constants.gameStart)) index++;
        return index-1;
    }

    public int GetBadEndingOffset(int episodeIndex, int choiceNum)
    {
        return badChoiceList[episodeIndex][choiceNum][0] - goodChoiceList[episodeIndex][choiceNum][0];
    }

    public int GetEpisodeSize(int episodeIndex)
    {
        return mainChatList[episodeIndex].Count;
    }

    public bool IsSavePoint(int episodeNum, int index)
    {
        if (GameManager.Instance.ChoiceNum < 0 || GameManager.Instance.ChoiceNum == goodChoiceList[episodeNum].Count - 1) return false;
        return (goodChoiceList[episodeNum][GameManager.Instance.ChoiceNum][0] == index);
    }


    // 마지막 분기인지 확인
    public bool IsLastDialogueSet(int episodeNum, int index)
    {
        if (episodeNum == 2) return (badChoiceList[episodeNum][badChoiceList[episodeNum].Count - 1][1] < index);
        else return (badChoiceList[episodeNum][badChoiceList[episodeNum].Count - 2][1] < index);
    }

    public bool IsLastBadEndingChat(int episodeNum, int index)
    {
        return (index == badChoiceList[episodeNum][badChoiceList[episodeNum].Count - 1][1]);
    }

    // 해당 채팅.index 값이 범위에 속하는지 판단(반대 범위에 속하지 않아야 참을 반환)
    public bool IsInRange(int chatIndex, bool isGoodChoice, int episodeNum, int choiceNum)
    {
        int cnum = choiceNum;
        if (cnum >= goodChoiceList[episodeNum].Count) cnum = goodChoiceList[episodeNum].Count - 1;

        if (isGoodChoice)
        {
            return !(chatIndex >= badChoiceList[episodeNum][cnum][0] && chatIndex <= badChoiceList[episodeNum][cnum][1]);
        }
        else
        {
            return !(chatIndex >= goodChoiceList[episodeNum][cnum][0] && chatIndex <= goodChoiceList[episodeNum][cnum][1]);
        }
    }

    #region 데이터 로드

    private void LoadNoticeData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        for (int i=0; i<chatroomList.Count; i++)
        {
            string chatroomName = lines[i * 4].Substring(0,lines[i*4].Length);
            List<string> data = new List<string>();
            for(int j=1; j<=3; j++)
            {
                data.Add(lines[i*4+j]);
            }
            noticeList.Add(chatroomName, data);
        }
    }

    private void LoadEndingData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        int endingIndex = 0;
        string endingTxt = "";
        for(int i=0; i<lines.Length; i++)
        {
            if (lines[i].Contains("-"))
            {
                endingList.Add(endingTxt);
                endingTxt = "";
                endingIndex++;
            }
            else
            {
                if (!endingTxt.Equals("")) endingTxt += "\n";
                endingTxt += (lines[i]);
            }
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
        tutorialChat = CSVReader.Read("chatData_tutorial");

        for (int i = 0; i < 3; i++)
        {
            mainChatList.Add(CSVReader.Read("chatData_ep" + (i+1)));

            if (i != 2)
            {
                //개인톡 따로 저장 후 메인리스트에서 삭제
                for(int j=0; j< mainChatList[i].Count; j++)
                {
                    personalChatList.Add(new List<ChatData>());
                    if (!mainChatList[i][j].chatroom.Equals(Constants.grouptalk))
                    {
                        personalChatList[i].Add(mainChatList[i][j]);
                    }
                }
                for(int j=0; j<personalChatList[i].Count; j++)
                {
                    mainChatList[i].RemoveAt(mainChatList[i].Count - 1);
                }
            }

            int index = 0;
            List<ChatData> allSubChatList = CSVReader.Read("chatData_SubEp" + (i + 1) + "Str");
            subChatList.Add(new List<List<ChatData>>());
            subChatList[i].Add(new List<ChatData>());
            subChatList[i][index].Add(allSubChatList[0]);
            for (int j=1; j<allSubChatList.Count; j++)
            {
                // 채팅방이 변경되었으면 다음 인덱스에 저장
                if (!allSubChatList[j - 1].chatroom.Equals(allSubChatList[j].chatroom))
                {
                    subChatList[i].Add(new List<ChatData>());
                    index++;
                }
                subChatList[i][index].Add(allSubChatList[j]);
            }
        }
    }
    #endregion 데이터 로드 --------------------------------------------------------------------------------------
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
