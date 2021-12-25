using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OldGameManager : MonoBehaviour
{
    [Header("��ġ ����")]
    [Range(1, 10)]
    [SerializeField] private int speed;   // ä�� ���� �ӵ�

    [SerializeField] private Button goMainBtn;

    private static OldGameManager instance;

    private List<Text> readCountTxt = new List<Text>();

    private AudioController audioController;
    private ChatListUI chatListUI;
    private ChatUI chatUI;


    // ���̺� ����: ������ ���� ��, ���Ǽҵ� ��ȯ, 
    #region SAVE_DATA
    private bool isTutorialFinished = false;                // Ʃ�丮�� �Ϸ� ����
    private bool isEpisodeFinished = false;                 // ���� ���Ǽҵ� �Ϸ� ����(�Ϸ� �� ä�� ���� ����; ���Ǽҵ� ��ȯ �� ����)
    private bool isChoiceRight;                              // ���� �������� ���� ������ ��������
    private int episodeIndex = -1;                          // ���� ���Ǽҵ� ��ȣ
    private int choiceNum = -1;                              // ���� ���Ǽҵ忡�� ���������� �Ϸ��� ������ ��ȣ
    private int currentChatIndex = 0;                        // ���� ����� ä�� ��ȣ(���̺�����Ʈ: ������ ����)
    private int groupTalkUnChecked = 3;                      // (����� ����)������ ��� ��
    private ENDING ending = ENDING.NORMAL;                   // ����
    #endregion --------------------------------------------------------------------------------------------------

    private ChatData currentChatData;                       // ����� ä�� ������
    private bool isWait = false;                            // ������
    private int lastChatIndex = -1;                          // ���� �ֱ� ���ŵ� ä�� ��ȣ
    private float spendTime;                                // �帥 �ð�(ä�� ��� �� ����)
    private CHATROOM chatroom = CHATROOM.NULL;              // ���� ������ ä�ù� �̸�(������,���ȣ,������,��ä��,����,4MasterTalk,GameMasters)
    private SCENE currentScene;                             // ���� �� Ÿ��


    public List<int> mainEpisodeCnt;

    public enum ENDING { NORMAL, BAD1, BAD2, BAD3, }
    public enum SCENE { MAIN, CHATLIST, INGAME, ENDING, }
    public enum CHATROOM { ������, ���ȣ, ������, ��ä��, ����, MasterTalk, GameMasters, NULL }    // * MasterTalk => 4MasterTalk (���ڷ� ������ �� ����)

    #region ������Ƽ

    public int Speed
    {
        get { return speed; }
    }
    public AudioController Audio
    {
        get { return audioController; }
    }
    public List<Text> ReadCountTxt
    {
        get { return readCountTxt; }
    }

    public bool IsTutorialFinished
    {
        get { return isTutorialFinished; }
    }
    public bool IsEpisodeFinished
    {
        get { return isEpisodeFinished; }
    }
    public bool IsChoiceRight
    {
        get { return isChoiceRight; }
        set { isChoiceRight = value; }
    }
    public int EpisodeIndex
    {
        get { return episodeIndex; }
    }
    public int ChoiceNum
    {
        get { return choiceNum; }
        set { choiceNum = value; }
    }
    public int CurrentChatIndex
    {
        get { return currentChatIndex; }
    }
    public int GroupTalkUnChecked
    {
        get { return groupTalkUnChecked; }
        set { groupTalkUnChecked = value; }
    }

    public ENDING Ending
    {
        get { return ending; }
    }


    public bool IsWait
    {
        get { return isWait; }
        set { isWait = value; }
    }

    public CHATROOM Chatroom
    {
        get { return chatroom; }
        set { chatroom = value; }
    }

    public int LastChatIndex
    {
        get { return lastChatIndex; }
        set { lastChatIndex = value; }
    }
    #endregion ������Ƽ -----------------------------------------------------------------------------------

    public static OldGameManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(OldGameManager)) as OldGameManager;

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

        audioController = GetComponent<AudioController>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += LoadedSceneEvent;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LoadedSceneEvent;
    }

    private void Update()
    {
        spendTime += (Time.deltaTime * speed);

        //(��忣�� ������ �Ǵ� ���Ǽҵ�2�� ������) ä�� ����� �����ٸ� ä����� �ߴ�
        if ((!isEpisodeFinished) &&
             ((ending != ENDING.NORMAL && DataManager.Instance.IsLastBadEndingChat(episodeIndex, currentChatData.index)) ||
               (episodeIndex == 2 && currentChatIndex >= mainEpisodeCnt[episodeIndex] - 1)))
        {
            if (!isEpisodeFinished) isEpisodeFinished = true;
        }
        else if (episodeIndex >= 0 && !isWait && spendTime >= currentChatData.dt)
        {
            // ���Ǽҵ� �������̰�
            // ���� ä�ù濡 �ְų� ä�ù濡 ��� ����Ǵ� �������� ���
            int characterIndex = DataManager.Instance.characterList.IndexOf(currentChatData.character);
            if (currentChatIndex < mainEpisodeCnt[episodeIndex] &&
                (chatroom.ToString() == currentChatData.chatroom || (characterIndex > 2 && characterIndex < 13)))
            {
                GoNext();
            }
        }

        // ��ġ��
        if (Input.GetMouseButtonDown(0))
        {
            audioController.PlayEffect(AudioController.EFFECT.TOUCH);
        }
        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        audioController.PlayEffect(AudioController.EFFECT.TOUCH);
        //    }
        //}
    }

    public void GoNext()
    {
        // ���� ���Ǽҵ� ������ ����Ǿ��� ��� - ���� ���Ǽҵ�� �̵� or ���ӿ���
        if (currentChatIndex >= DataManager.Instance.GetEpisodeSize(episodeIndex) - 1)
        {
            if (!isEpisodeFinished)
            {
                isEpisodeFinished = true;
                //Debug.Log("GoNext: episode finished");
            }
        }
        else
        {
            // ������ �����̸� ���̺�
            if (DataManager.Instance.IsSavePoint(episodeIndex, currentChatData.index)) Save();

            currentChatIndex++;
            currentChatData = DataManager.Instance.GetMainChat(episodeIndex, currentChatIndex);
            spendTime = 0f;

            //Debug.Log("GoNext:"+currentChatIndex);
            string waitCharacter = "����,������";
            if (waitCharacter.Contains(currentChatData.character)) isWait = true;
            //if (currentScene != SCENE.INGAME && currentChatData.character == "��Ʈ��") isWait = true;
        }

    }

    public void GoToNextEpisode()
    {
        ChangeScene(SCENE.CHATLIST);
        episodeIndex++;
        currentChatIndex = 0;
        currentChatData = DataManager.Instance.GetMainChat(episodeIndex, currentChatIndex);
        lastChatIndex = -1;
        choiceNum = -1;
        groupTalkUnChecked = 3;
        isEpisodeFinished = false;
        ChatListManager.Instance.ResetState();
        audioController.SetNextIngameBGM(episodeIndex);
        ClearReadCount();
        Save();
    }

    public void Save()
    {
        int tutorialState = (isTutorialFinished) ? 1 : 0;
        int episodeState = (isEpisodeFinished) ? 1 : 0;
        int choiceRight = (isChoiceRight) ? 1 : 0;
        PlayerPrefs.SetInt("IsTutorialFinished", tutorialState);
        PlayerPrefs.SetInt("IsEpisodeFinished", episodeState);
        PlayerPrefs.SetInt("IsChoiceRight", choiceRight);

        PlayerPrefs.SetInt("Episode", episodeIndex);
        PlayerPrefs.SetInt("ChoiceNum", choiceNum);
        PlayerPrefs.SetInt("ChatIndex", currentChatIndex);
        PlayerPrefs.SetInt("GroupTalkUnChecked", groupTalkUnChecked);
        PlayerPrefs.SetInt("Ending", (int)ending);

        ChatListManager.Instance.Save();

        Debug.Log("saved:" + currentChatIndex);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("Episode"))
        {
            isTutorialFinished = (PlayerPrefs.GetInt("IsTutorialFinished") == 1);
            isEpisodeFinished = (PlayerPrefs.GetInt("isEpisodeFinished") == 1);
            isChoiceRight = (PlayerPrefs.GetInt("IsChoiceRight") == 1);

            episodeIndex = PlayerPrefs.GetInt("Episode");
            choiceNum = PlayerPrefs.GetInt("ChoiceNum");
            currentChatIndex = PlayerPrefs.GetInt("ChatIndex");
            groupTalkUnChecked = PlayerPrefs.GetInt("GroupTalkUnChecked");
            ending = (ENDING)PlayerPrefs.GetInt("Ending");

            // �� ä�ù��� ���� ���� �ҷ�����
            for (int i = 0; i < DataManager.Instance.chatroomList.Count; i++)
            {
                bool value = (PlayerPrefs.GetInt("IsEntered_" + i) == 1);
                ChatListManager.Instance.SetIsEntered(i, value);
            }

            audioController.LoadIngameBGM();

            // �ε� �Ϸ�--------------------------------------------------------------

            // ������ ���� ä�ý������� ����
            if (ending != ENDING.NORMAL) currentChatIndex += DataManager.Instance.GetBadEndingOffset(episodeIndex, choiceNum);
            lastChatIndex = currentChatIndex - 1;

            currentChatData = DataManager.Instance.GetMainChat(episodeIndex, currentChatIndex);
            Debug.Log(
                "[Loaded]" +
                " currentChatIndex:" + currentChatIndex +
                ", episodeIndex:" + episodeIndex +
                ", choiceNum:" + choiceNum +
                ", ending:" + ending);
        }
    }

    public void ResetGame()
    {
        Debug.Log("���ӵ����� ����");

        // PlayerPrefs ������ ����(���� ������ ����)
        float bgmVolume = audioController.GetBgmVolume();
        float effectVolume = audioController.GetEffectVolume();
        PlayerPrefs.DeleteAll();
        audioController.SetBgmVolume(bgmVolume);
        audioController.SetEffectVolume(effectVolume);

        isTutorialFinished = false;
        isEpisodeFinished = false;

        episodeIndex = -1;
        choiceNum = -1;
        currentChatIndex = 0;
        groupTalkUnChecked = 3;
        ending = ENDING.NORMAL;

        ChatListManager.Instance.ResetState();

        isWait = false;
        lastChatIndex = -1;
        spendTime = 0f;
    }

    public void FinishTutorial()
    {
        isTutorialFinished = true;
        GoToNextEpisode();
    }

    public void SkipTime()
    {
        spendTime += 100f;
    }

    public void AddReadCountTxt(Text text)
    {
        readCountTxt.Add(text);
    }

    public void ClearReadCount()
    {
        readCountTxt.Clear();
    }

    // ���� ������ ���� �� ���� ���Ǽҵ� ��ȣ�� ������ȣ�� ����
    public void ChangeEnding()
    {
        if (ending == (ENDING)(episodeIndex + 1)) return;
        Debug.Log("���� ������");
        ending = (ENDING)(episodeIndex + 1);
        audioController.ChangeMood(episodeIndex);
    }

    public void ChangeScene(SCENE scene)
    {
        //audioController.StopBGM();
        SceneManager.LoadScene((int)scene);
    }

    // �� �ε� �� ȣ��
    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " �� �ε��");

        // �޴�â '��������' ��ư Ȱ��ȭ ���� ����
        goMainBtn.interactable = (scene.buildIndex != (int)SCENE.MAIN);

        if (scene.buildIndex == (int)SCENE.MAIN)
        {
            currentScene = SCENE.MAIN;
            Load();
            audioController.PlayBGM(AudioController.BGM.MAIN);
        }

        if (scene.buildIndex == (int)SCENE.CHATLIST)
        {
            currentScene = SCENE.CHATLIST;
            if (chatListUI == null) chatListUI = GameObject.Find("Canvas").GetComponent<ChatListUI>();
            audioController.PlayBGM(AudioController.BGM.MAIN);
        }

        // �ΰ���(ä��â)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI == null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
            if (chatroom == CHATROOM.������) audioController.PlayIngameBGM();
            else
            {
                string bgmName = "EP" + (episodeIndex + 1);
                audioController.PlayBGM((AudioController.BGM)System.Enum.Parse(typeof(AudioController.BGM), bgmName));
            }
        }

        // ����
        if (scene.buildIndex == (int)SCENE.ENDING)
        {
            currentScene = SCENE.ENDING;
            switch (ending)
            {
                case ENDING.NORMAL:
                    audioController.PlayBGM(AudioController.BGM.ED0);
                    break;
                case ENDING.BAD1:
                    audioController.PlayBGM(AudioController.BGM.ED1);
                    break;
                case ENDING.BAD2:
                    audioController.PlayBGM(AudioController.BGM.ED2);
                    break;
                case ENDING.BAD3:
                    audioController.PlayBGM(AudioController.BGM.ED3);
                    break;
                default:
                    break;
            }
        }
    }
}
