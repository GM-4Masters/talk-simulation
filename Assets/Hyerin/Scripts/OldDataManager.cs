using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldDataManager : MonoBehaviour
{
    private static OldDataManager instance;

    //private List<ChatData> chatDataList;
    private List<List<ChatData>> mainChatList = new List<List<ChatData>>();
    private List<List<ChatData>> subChatList = new List<List<ChatData>>();
    private List<ChatData> tutorialChat = new List<ChatData>();
    private List<List<ChatData>> personalChatList = new List<List<ChatData>>();

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

    public static OldDataManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(OldDataManager)) as OldDataManager;

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

    public List<ChatData> GetChatList(int episodeIndex, DATATYPE type)
    {
        List<ChatData> data = new List<ChatData>();
        if (type == DATATYPE.MAIN) data = mainChatList[episodeIndex];
        else if (type == DATATYPE.SUB) data = subChatList[episodeIndex];
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
                if (subChatList[episodeIndex][i].chatroom == chatroom.ToString())
                    data.Add(subChatList[episodeIndex][i]);
            }
        }

        return data;
    }

    public ChatData GetMainChat(int episodeIndex, int chatIndex)
    {
        return mainChatList[episodeIndex][chatIndex];
        //return chatDataList[chatIndex];
    }

    public ChatData GetLastMainChat(bool isGoodEnding, int episodeNum)
    {
        // 엔딩에 따라 끝지점 지정
        int last = (isGoodEnding) ?
            goodChoiceList[episodeNum][GameManager.Instance.ChoiceNum][1] :
            badChoiceList[episodeNum][GameManager.Instance.ChoiceNum][1];

        // 끝에서부터 뒤로가면서 끝지점 인덱스보다 작고, 단톡방이고, 사람인 가장 첫 데이터 리턴
        for (int i = mainChatList[episodeNum].Count - 1; i >= 0; i--)
        {
            int charIndex = characterList.IndexOf(mainChatList[episodeNum][i].character);
            if (mainChatList[episodeNum][i].index <= last &&
                mainChatList[episodeNum][i].chatroom == chatroomList[0] &&
                (charIndex > 6 || charIndex == 3))
            {
                return mainChatList[episodeNum][i];
            }
        }
        return null;
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
        return personalChatList[episodeNum][0].character;
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
    public bool IsInRange(int chatIndex, bool isGoodChoice, int episodeNum)
    {
        bool result = true;
        //Debug.Log("idx:" + chatIndex + ", Good:" + isGoodChoice + ", cho:" + choiceNum+", min:"+
        //    goodChoiceList[episodeNum][choiceNum][0]+", max:"+ goodChoiceList[episodeNum][choiceNum][1]);
        if (isGoodChoice)
        {
            for (int i = 0; i < badChoiceList[episodeNum].Count; i++)
            {
                if (chatIndex >= badChoiceList[episodeNum][i][0] && chatIndex <= badChoiceList[episodeNum][i][1]) result = false;
            }
        }
        else
        {
            for (int i = 0; i < goodChoiceList[episodeNum].Count; i++)
            {
                if (chatIndex >= goodChoiceList[episodeNum][i][0] && chatIndex <= goodChoiceList[episodeNum][i][1]) result = false;
            }
        }

        return result;

        //if (isGoodChoice) return (chatIndex >= goodChoiceList[episodeNum][choiceNum][0] && chatIndex <= goodChoiceList[episodeNum][choiceNum][1]);
        //else return (chatIndex >= badChoiceList[episodeNum][choiceNum][0] && chatIndex <= badChoiceList[episodeNum][choiceNum][1]);
    }

    #region 데이터 로드

    private void LoadNoticeData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        for (int i = 0; i < chatroomList.Count; i++)
        {
            List<string> data = new List<string>();
            for (int j = 0; j < 3; j++)
            {
                data.Add(lines[i * 3 + j]);
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
            else endingTxt += (line + "\n");
        }
    }

    private void LoadChoiceData(string fileName)
    {
        TextAsset txt = Resources.Load(fileName) as TextAsset;
        string[] lines = txt.text.Split('\n');

        List<int[]> data;
        for (int i = 0; i < 3; i++)
        {
            data = new List<int[]>();
            string[] goodChoices = lines[i].Split(' ');
            for (int j = 0; j < goodChoices.Length; j++)
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
            mainChatList.Add(CSVReader.Read("chatData_ep" + (i + 1)));

            if (i != 2)
            {
                //개인톡 따로 저장
                for (int j = 0; j < mainChatList[i].Count; j++)
                {
                    personalChatList.Add(new List<ChatData>());
                    if (mainChatList[i][j].chatroom != "팀단톡")
                    {
                        personalChatList[i].Add(mainChatList[i][j]);
                    }
                }

                // 에피소드 1,2 마지막 개인톡 3개 잘라냄
                for (int j = 0; j < 3; j++)
                {
                    mainChatList[i].RemoveAt(mainChatList[i].Count - 1);
                }
            }

            subChatList.Add(CSVReader.Read("chatData_SubEp" + (i + 1) + "Str"));
        }

        GameManager.Instance.mainEpisodeCnt = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            GameManager.Instance.mainEpisodeCnt.Add(GetEpisodeSize(i));
        }
    }
    #endregion 데이터 로드 --------------------------------------------------------------------------------------
}
