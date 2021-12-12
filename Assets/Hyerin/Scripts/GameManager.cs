using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("��ġ ����")]
    [Range(1, 10)]
    public int speed;   // ä�� ���� �ӵ�

    [SerializeField] private Button goMainBtn;

    private static GameManager instance;

    private List<Text> readCountTxt = new List<Text>();

    private AudioController audioController;
    private ChatListUI chatListUI;
    private ChatUI chatUI;


    // ���̺� ����: ������ ���� ��, ���Ǽҵ� ��ȯ, 
    #region SAVE_DATA ------------------------------------------------------------------------------------------
    private bool isTutorialFinished = false;                // Ʃ�丮�� �Ϸ� ����
    private bool isEpisodeFinished = false;                 // ���� ���Ǽҵ� �Ϸ� ����(�Ϸ� �� ä�� ���� ����; ���Ǽҵ� ��ȯ �� ����)
    public bool isChoiceRight;                              // ���� �������� ���� ������ ��������
    private int episodeIndex = -1;                          // ���� ���Ǽҵ� ��ȣ
    public int choiceNum = -1;                              // ���� ���Ǽҵ忡�� ���������� �Ϸ��� ������ ��ȣ
    public int currentChatIndex = 0;                        // ���� ����� ä�� ��ȣ
    public int groupTalkUnChecked = 3;                      // (����� ����)������ ��� ��
    public ENDING ending = ENDING.NORMAL;                   // ����
    #endregion --------------------------------------------------------------------------------------------------

    private ChatData currentChatData;                       // ����� ä�� ������(���̺�����Ʈ: ������ ����)
    private bool isWait = false;                            // ������
    public int lastChatIndex = -1;                          // ���� �ֱ� ���ŵ� ä�� ��ȣ
    private float spendTime;                                // �帥 �ð�(ä�� ��� �� ����)
    public string chatroom = null;                          // ���� ������ ä�ù� �̸�(������,���ȣ,������,��ä��,����,4MasterTalk,GameMasters)
    private SCENE currentScene;                             // ���� �� Ÿ��


    public List<int> mainEpisodeCnt;

    public enum ENDING { NORMAL, BAD1, BAD2, BAD3, }
    public enum SCENE { MAIN, CHATLIST, INGAME, ENDING, }

    public static GameManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;

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
        spendTime += (Time.deltaTime*speed);

        //(��忣�� ������ �Ǵ� ���Ǽҵ�2�� ������) ä�� ����� �����ٸ� ä����� �ߴ�
        if ( (!isEpisodeFinished) &&
             ( (ending != ENDING.NORMAL && DataManager.Instance.IsLastBadEndingChat(currentChatData.index) ) ||
               (episodeIndex == 2 && currentChatIndex >= mainEpisodeCnt[episodeIndex] - 1) ) )
        {
            if (!isEpisodeFinished) isEpisodeFinished = true;
        }
        else if (episodeIndex >= 0 && !isWait && spendTime >= currentChatData.dt)
        {
            // ���Ǽҵ� �������̰�
            // ���� ä�ù濡 �ְų� ä�ù濡 ��� ����Ǵ� �������� ���
            int characterIndex = DataManager.Instance.characterList.IndexOf(currentChatData.character);
            if (currentChatIndex < mainEpisodeCnt[episodeIndex] &&
                (chatroom == currentChatData.chatroom || (characterIndex > 2 && characterIndex < 13)))
            {
                GoNext();
            }
        }

        // ��ġ��
        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.TOUCH);
        //    }
        //}
    }

    public void GoNext()
    {
        // ���� ���Ǽҵ� ������ ����Ǿ��� ��� - ���� ���Ǽҵ�� �̵� or ���ӿ���
        if (currentChatIndex >= DataManager.Instance.GetEpisodeSize(episodeIndex)-1)
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
            if (DataManager.Instance.IsSavePoint(currentChatData.index, episodeIndex)) Save();

            currentChatIndex++;
            currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
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
        currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
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
        int tutorialState = (isTutorialFinished)? 1 : 0;
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
            if (ending != ENDING.NORMAL) currentChatIndex += DataManager.Instance.GetBadEndingOffset(episodeIndex,choiceNum);
            lastChatIndex = currentChatIndex-1;

            currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
            //Debug.Log("episodeIndex:" + episodeIndex+ ", currentChatIndex:"+ currentChatIndex);
            Debug.Log("Loaded:" + currentChatIndex + ", ending:" + ending);
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

    public float GetPlaySpeed()
    {
        return speed;
    }

    public int GetEpisodeIndex()
    {
        return episodeIndex;
    }

    public SCENE GetCurrentScene()
    {
        return currentScene;
    }

    public ChatData GetChatData()
    {
        return currentChatData;
    }

    public ENDING GetEndingType()
    {
        return ending;
    }

    public List<Text> GetReadCountTxt()
    {
        return readCountTxt;
    }

    public AudioController GetAudioController()
    {
        return audioController;
    }

    public bool IsEpisodeFinished()
    {
        return isEpisodeFinished;
    }

    public bool IsTutorialFinished()
    {
        return isTutorialFinished;
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
        ending = (ENDING)(episodeIndex+1);
        audioController.ChangeMood(episodeIndex);
    }

    public void ChangeWaitFlag(bool value)
    {
        isWait = value;
    }

    public void ChangeScene(SCENE scene)
    {
        audioController.StopBGM();
        SceneManager.LoadScene((int)scene);
    }

    public void ChangeChatroom(int index)
    {
        if (index == -1) chatroom = null;
        else chatroom = DataManager.Instance.chatroomList[index];
    }

    // �� �ε� �� ȣ��
    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " �� �ε��");

        // �޴�â '��������' ��ư Ȱ��ȭ ���� ����
        goMainBtn.interactable = (scene.buildIndex!=(int)SCENE.MAIN);

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
        }

        // �ΰ���(ä��â)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI == null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
            if(chatroom==DataManager.Instance.chatroomList[0]) audioController.PlayIngameBGM();
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
