using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    [Header("��ġ ����")]
    [Range(1, 10)]
    public int monologueSpeed;   // ���� Ÿ���� �ӵ�

    private static GameManager instance;
    private AudioClip[] audioClip = new AudioClip[3];
    private AudioSource audioSource;

    private ChatListUI chatListUI;
    private ChatUI chatUI;

    private ChatData currentChatData;                       // ����� ä�� ������

    public int lastListIndex = -1;
    public int lastChatIndex = -1;

    private bool isWait;                           // ������
    private float spendTime;                                // �帥 �ð�(ä�� ��� �� ����)
    public int currentChatIndex = 0;                       // ���� ����� ä�� ��ȣ
    private int episodeIndex = 0;                           // ���� ���Ǽҵ� ��ȣ
    private SCENE currentScene;                             // ���� �� Ÿ��
    public ENDING ending = ENDING.NORMAL;                   // ����

    public int[] boundary = { 30, 60, 90 };                 // ���Ǽҵ� �б���

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

        //// ���� ���Ǽҵ� ��� �Ǵ� ���ӿ���
        //if (currentChatIndex == boundary[episodeIndex])
        //{
        //    if (ending != ENDING.NORMAL) ShowEnding();
        //    else episodeIndex++;
        //}

        // �ð����� �����߰�
        if (spendTime >= currentChatData.dt)
        {
            // ���� ä�ù濡 �ְų� ä�ù濡 ��� ����Ǵ� �������� ���
            int characterIndex = DataManager.Instance.characterList.IndexOf(currentChatData.character);
            if (currentScene == SCENE.INGAME || (characterIndex > 3 && characterIndex < 13))
            {
                currentChatIndex++;
                currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
                spendTime = 0f;
            }
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Episode", episodeIndex);
        PlayerPrefs.SetInt("ChatIndex", currentChatIndex);
        PlayerPrefs.SetInt("Ending", (int)ending);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("Episode"))
        {
            episodeIndex = PlayerPrefs.GetInt("Episode");
            currentChatIndex = PlayerPrefs.GetInt("ChatIndex");
            ending = (ENDING)PlayerPrefs.GetInt("Ending");
        }
    }

    public void ResetGame()
    {
        Debug.Log("���ӵ����� ����");

        PlayerPrefs.DeleteAll();
        isWait = false;
        episodeIndex = 0;
        spendTime = 0f;
        currentChatIndex = 0;
        ending = ENDING.NORMAL;
    }

    public ChatData GetChatData()
    {
        return currentChatData;
    }
    
    public ENDING GetEndingType()
    {
        return ending;
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
            if (chatListUI == null)  chatListUI = GameObject.Find("Canvas").GetComponent<ChatListUI>();
        }

        // �ΰ���(ä��â)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI==null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
        }

        // ����
        if (scene.buildIndex == (int)SCENE.ENDING)
        {
            currentScene = SCENE.ENDING;
            if (ending==ENDING.NORMAL) PlayAudio(AUDIO.NORMAL);
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
