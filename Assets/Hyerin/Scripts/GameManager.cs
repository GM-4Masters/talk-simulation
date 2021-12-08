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
    public int monologueSpeed;   // ���� Ÿ���� �ӵ�

    private static GameManager instance;
    private AudioClip[] audioClip = new AudioClip[3];
    private AudioSource audioSource;

    private List<Text> readCountTxt = new List<Text>();

    private ChatListUI chatListUI;
    private ChatUI chatUI;

    private ChatData currentChatData;                       // ����� ä�� ������(���̺�����Ʈ: ������ ����)

    private bool isTutorialFinished = false;               // Ʃ�丮�� �Ϸ� ����
    private bool isEpisodeFinished = false;                 // ���� ���Ǽҵ� �Ϸ� ����(�Ϸ� �� ä�� ���� ����; ���Ǽҵ� ��ȯ �� ����)
    private bool isWait;                                    // ���

    public int selectedNum = -1;                            // ���� ���Ǽҵ忡�� ������ ��ģ Ƚ��
    public int currentChatIndex = 0;                        // ���� ����� ä�� ��ȣ
    public int lastChatIndex = -1;                          // ���� �ֱ� ����� ä�� ��ȣ
    public int groupTalkUnChecked = 3;                      // (����� ����)������ ��� ��
    private int episodeIndex = -1;                          // ���� ���Ǽҵ� ��ȣ
    private float spendTime;                                // �帥 �ð�(ä�� ��� �� ����)
    public string chatroom = null;                          // ���� ������ ä�ù� �̸�(������,���ȣ,������,��ä��,����,Ʃ�丮��,GameMasters)
    private SCENE currentScene;                             // ���� �� Ÿ��
    public ENDING ending = ENDING.NORMAL;                   // ����

    public List<List<int>> selectIndex = new List<List<int>>(){ new List<int>{10, 34, 69},
                                                                new List<int>{19,49,69,90,126 },
                                                                new List<int>{29,47,67,90,115,136 }};

    //�ӽ�
    public int[,,] epGood = {   { { 10, 12 }, { 34, 39 }, { 69, 70 }, { 83, 87 } },
                                { { 10, 12 }, { 34, 39 }, { 69, 70 }, { 83, 87 } },
                                { { 10, 12 }, { 34, 39 }, { 69, 70 }, { 83, 87 } },};
    public int[,,] epBad = {    { { 13, 17 }, { 40, 46 }, { 71, 82 }, { 88, 89 } },
                                { { 13, 17 }, { 40, 46 }, { 71, 82 }, { 88, 89 } },
                                { { 13, 17 }, { 40, 46 }, { 71, 82 }, { 88, 89 } } };

    public List<int> mainEpisodeCnt;

    public enum ENDING { BAD1, BAD2, BAD3, NORMAL, }
    public enum AUDIO { MAIN, INGAME, BAD, NORMAL, NOTIFICATION }
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

        audioSource = GetComponent<AudioSource>();
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
        spendTime += Time.deltaTime;
        // ���� �����߰�
        if (episodeIndex >= 0 && !isWait && spendTime >= currentChatData.dt)
        {
            // ���� ä�ù濡 �ְų� ä�ù濡 ��� ����Ǵ� �������� ���
            int characterIndex = DataManager.Instance.characterList.IndexOf(currentChatData.character);
            if (currentChatIndex<mainEpisodeCnt[episodeIndex] &&
                currentScene == SCENE.INGAME || (characterIndex > 3 && characterIndex < 13))
            {
                GoNext();
            }
        }
    }

    public void GoNext()
    {
        // ���� ���Ǽҵ� ������ ����Ǿ��� ��� - ���� ���Ǽҵ�� �̵� or ���ӿ���
        if (currentChatIndex == DataManager.Instance.GetEpisodeSize(episodeIndex)-1)
        {
            if (ending == ENDING.NORMAL)
            {
                if (!isEpisodeFinished)
                {
                    isEpisodeFinished = true;
                }
            }
            else if(currentScene != SCENE.ENDING) ChangeScene(SCENE.ENDING);
        }
        else if(currentChatIndex < mainEpisodeCnt[episodeIndex]-1)
        {
            currentChatIndex++;
            currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
            spendTime = 0f;

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
        selectedNum = -1;
        groupTalkUnChecked = 3;
        isEpisodeFinished = false;
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Episode", episodeIndex);
        PlayerPrefs.SetInt("ChatIndex", currentChatIndex);
        PlayerPrefs.SetInt("SelectedNum", selectedNum);
        PlayerPrefs.SetInt("Ending", (int)ending);
        PlayerPrefs.SetInt("GroupTalkUnChecked", groupTalkUnChecked);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("Episode"))
        {
            episodeIndex = PlayerPrefs.GetInt("Episode");
            currentChatIndex = PlayerPrefs.GetInt("ChatIndex");
            selectedNum = PlayerPrefs.GetInt("SelectedNum");
            ending = (ENDING)PlayerPrefs.GetInt("Ending");
            groupTalkUnChecked = PlayerPrefs.GetInt("GroupTalkUnChecked");
            if (ending != ENDING.NORMAL) currentChatIndex += (epBad[episodeIndex,selectedNum,0]-epGood[episodeIndex, selectedNum,0]);
            lastChatIndex = currentChatIndex-1;
        }
    }

    public void ResetGame()
    {
        Debug.Log("���ӵ����� ����");

        PlayerPrefs.DeleteAll();
        isTutorialFinished = false;
        isWait = false;
        episodeIndex = -1;
        spendTime = 0f;
        currentChatIndex = 0;
        lastChatIndex = -1;
        ending = ENDING.NORMAL;
    }

    public int GetEpisodeIndex()
    {
        return episodeIndex;
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

    // ���� ���� �帧�� �´� ��ȭ����
    public bool IsRight(int index)
    {
        if (selectedNum < 0) return true;
        // ��忣�� �ƴѰ�� ��忣�� ä�� ������ ������ �ʾƾ� ��
        if (ending == ENDING.NORMAL)
        {
            //Debug.Log("index:" + index + ",epbad[0]"+epBad[selectedNum,0]+",epbad[1]:"+epb);
            return (index < epBad[episodeIndex, selectedNum, 0] || index > epBad[episodeIndex, selectedNum, 1]);
        }
        else
        {
            //Debug.Log("index:" + index + ",ep");
            return (index < epGood[episodeIndex, selectedNum, 0] || index > epGood[episodeIndex, selectedNum, 1]);
        }
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
        ending = (ENDING)episodeIndex;
    }

    public void ChangeWaitFlag(bool value)
    {
        isWait = value;
    }

    public void ChangeScene(SCENE scene)
    {
        audioSource.Stop();
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

        if (scene.buildIndex == (int)SCENE.MAIN)
        {
            currentScene = SCENE.MAIN;
            Load();
            currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
            PlayAudio(AUDIO.MAIN);
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
        }

        // ����
        if (scene.buildIndex == (int)SCENE.ENDING)
        {
            currentScene = SCENE.ENDING;
            if (ending == ENDING.NORMAL) PlayAudio(AUDIO.NORMAL);
            else PlayAudio(AUDIO.BAD);
        }
    }

    private void PlayAudio(AUDIO audio)
    {
        //if (audioSource.isPlaying) audioSource.Stop();
        //audioSource.clip = audioClip[(int)audio];
        //audioSource.Play();
    }

    public void PlayAnotherAudio(AUDIO audio)
    {

    }
}
