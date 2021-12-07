using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private ChatData currentTextData;       // 출력할 채팅 데이터

    //private bool isPlay;                    // 인게임 시간흐름 여부
    private float spendTime;                // 흐른 시간(채팅 출력 후 리셋)
    private int roomIndex;                  // 입장한 채팅방 번호
    private int currentTextIndex;           // 현재 출력할 채팅 번호
    private int episodeIndex;               // 현재 에피소드 번호
    private SCENE currentScene;             // 현재 씬 타입
    public ENDING ending;                   // 엔딩

    public int[] boundary = { 30, 60, 90 }; // 에피소드 분기점

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

        // 다음 에피소드 재생 또는 게임오버
        if (currentTextIndex == boundary[episodeIndex])
        {
            if (ending != ENDING.NORMAL) ShowEnding();
            else episodeIndex++;
        }
        //if (spendTime < currentTextData.dt)
        //{
        //    // 다음 채팅 출력
        //    if (currentScene == SCENE.CHATLIST)
        //    {
        //        if (episodeIndex == 0) PlayAnotherAudio(AUDIO.NOTIFICATION);
        //        chatListUI.RefreshChat(currentTextData);
        //    }
        //    else if (currentScene == SCENE.INGAME)
        //    {
        //        chatUI.RefreshChat(currentTextData);
        //    }
        //    currentTextIndex++;
        //    currentTextData = DataManager.Instance.GetChatData(episodeIndex, currentTextIndex);
        //    spendTime = 0f;
        //}
        //if (isPlay)
        //{

        //}
    }

    public void GameStart()
    {
        ChangeScene(SCENE.CHATLIST);
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        episodeIndex = 0;
        //isPlay = false;
        spendTime = 0f;
        currentTextIndex = 0;
        ending = ENDING.NORMAL;
}

    // 시간 재생
    public void Play()
    {
        //isPlay = true;
        Time.timeScale = 1f;
    }

    // 선택지 선택할 때까지 대기
    public void Wait()
    {
        Time.timeScale = 0f;
    }

    public void ShowEnding()
    {
        ChangeScene(SCENE.ENDING);
    }

    public int GetEpisodeIndex()
    {
        return episodeIndex;
    }
    
    public ENDING GetEndingType()
    {
        return ending;
    }

    // 엔딩조건 충족 시 현재 에피소드 번호를 엔딩번호로 지정
    public void ChangeEnding()
    {
        ending = (ENDING)episodeIndex;
    }

    public void SetRoomIndex(int index)
    {
        roomIndex = index;
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
            PlayAudio(AUDIO.MAIN);
        }
        
        if (scene.buildIndex == (int)SCENE.CHATLIST)
        {
            if (chatListUI == null)  chatListUI = GameObject.Find("Canvas").GetComponent<ChatListUI>();
        }

        // 인게임(채팅창)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            if(chatUI==null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
        }

        // 엔딩
        if (scene.buildIndex == (int)SCENE.ENDING)
        {
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
    
    private void PlayAnotherAudio(AUDIO audio)
    {

    }
}
