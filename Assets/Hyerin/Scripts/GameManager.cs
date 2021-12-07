using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    [Header("수치 설정")]
    [Range(1, 10)]
    public int monologueSpeed;   // 독백 타이핑 속도

    private static GameManager instance;
    private AudioClip[] audioClip = new AudioClip[3];
    private AudioSource audioSource;

    private ChatListUI chatListUI;
    private ChatUI chatUI;

    private ChatData currentChatData;                       // 출력할 채팅 데이터

    public int lastListIndex = -1;
    public int lastChatIndex = -1;

    private bool isWait;                           // 선택지
    private float spendTime;                                // 흐른 시간(채팅 출력 후 리셋)
    public int currentChatIndex = 0;                       // 현재 출력할 채팅 번호
    private int episodeIndex = 0;                           // 현재 에피소드 번호
    private SCENE currentScene;                             // 현재 씬 타입
    public ENDING ending = ENDING.NORMAL;                   // 엔딩

    public int[] boundary = { 30, 60, 90 };                 // 에피소드 분기점

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

        //// 다음 에피소드 재생 또는 게임오버
        //if (currentChatIndex == boundary[episodeIndex])
        //{
        //    if (ending != ENDING.NORMAL) ShowEnding();
        //    else episodeIndex++;
        //}

        // 시간조건 만족했고
        if (spendTime >= currentChatData.dt)
        {
            // 현재 채팅방에 있거나 채팅방에 없어도 실행되는 데이터일 경우
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
        Debug.Log("게임데이터 리셋");

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

    // 오답 선택지 선택 시 현재 에피소드 번호를 엔딩번호로 지정
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

    // 씬 로드 시 호출
    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " 씬 로드됨");

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

        // 인게임(채팅창)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI==null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
        }

        // 엔딩
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
