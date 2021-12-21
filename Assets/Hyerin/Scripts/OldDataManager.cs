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
        "����", "������", "���ӽ���", "��Ʈ��", "�ý���", "����", "����",
        "�����", "���ȣ", "������", "��ä��", "����", "GameMasters", "4MasterTalk" };

    public List<string> chatroomList = new List<string>()
    {
        "������", "���ȣ", "������", "��ä��", "����", "4MasterTalk", "GameMasters"
    };

    public List<List<string>> noticeList = new List<List<string>>();
    public List<string> endingList = new List<string>();

    // ���Ǽҵ�-��������ȣ
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
        // ������ ���� ������ ����
        int last = (isGoodEnding) ?
            goodChoiceList[episodeNum][GameManager.Instance.ChoiceNum][1] :
            badChoiceList[episodeNum][GameManager.Instance.ChoiceNum][1];

        // ���������� �ڷΰ��鼭 ������ �ε������� �۰�, ������̰�, ����� ���� ù ������ ����
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


    // ������ �б����� Ȯ��
    public bool IsLastDialogueSet(int episodeNum, int index)
    {
        if (episodeNum == 2) return (badChoiceList[episodeNum][badChoiceList[episodeNum].Count - 1][1] < index);
        else return (badChoiceList[episodeNum][badChoiceList[episodeNum].Count - 2][1] < index);
    }

    public bool IsLastBadEndingChat(int episodeNum, int index)
    {
        return (index == badChoiceList[episodeNum][badChoiceList[episodeNum].Count - 1][1]);
    }

    // �ش� ä��.index ���� ������ ���ϴ��� �Ǵ�(�ݴ� ������ ������ �ʾƾ� ���� ��ȯ)
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

    #region ������ �ε�

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
                //������ ���� ����
                for (int j = 0; j < mainChatList[i].Count; j++)
                {
                    personalChatList.Add(new List<ChatData>());
                    if (mainChatList[i][j].chatroom != "������")
                    {
                        personalChatList[i].Add(mainChatList[i][j]);
                    }
                }

                // ���Ǽҵ� 1,2 ������ ������ 3�� �߶�
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
    #endregion ������ �ε� --------------------------------------------------------------------------------------
}
