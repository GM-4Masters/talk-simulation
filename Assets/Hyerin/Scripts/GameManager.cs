using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("수치 설정")]
    [Range(1, 10)]
    public int monologueSpeed;   // 독백 타이핑 속도

    private static GameManager instance;
    private AudioClip[] audioClip = new AudioClip[3];
    private AudioSource audioSource;

    private List<Text> readCountTxt = new List<Text>();

    private ChatListUI chatListUI;
    private ChatUI chatUI;

    private ChatData currentChatData;                       // 출력할 채팅 데이터(세이브포인트: 선택지 이후)

    private bool isTutorialFinished = false;               // 튜토리얼 완료 여부
    private bool isEpisodeFinished = false;                 // 현재 에피소드 완료 여부(완료 후 채팅 띄우기 위해; 에피소드 전환 시 리셋)
    private bool isWait;                                    // 대기

    public int selectedNum = -1;                            // 현재 에피소드에서 선택지 거친 횟수
    public int currentChatIndex = 0;                        // 현재 출력할 채팅 번호
    public int lastChatIndex = -1;                          // 가장 최근 출력한 채팅 번호
    public int groupTalkUnChecked = 3;                      // (단톡방 한정)안읽은 사람 수
    private int episodeIndex = -1;                          // 현재 에피소드 번호
    private float spendTime;                                // 흐른 시간(채팅 출력 후 리셋)
    public string chatroom = null;                          // 현재 입장한 채팅방 이름(팀단톡,김산호,벅찬우,이채린,엄마,튜토리얼,GameMasters)
    private SCENE currentScene;                             // 현재 씬 타입
    public ENDING ending = ENDING.NORMAL;                   // 엔딩

    public List<List<int>> selectIndex = new List<List<int>>(){ new List<int>{10, 34, 69},
                                                                new List<int>{19,49,69,90,126 },
                                                                new List<int>{29,47,67,90,115,136 }};

    //임시
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
        // 조건 만족했고
        if (episodeIndex >= 0 && !isWait && spendTime >= currentChatData.dt)
        {
            // 현재 채팅방에 있거나 채팅방에 없어도 실행되는 데이터일 경우
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
        // 현재 에피소드 끝까지 진행되었을 경우 - 다음 에피소드로 이동 or 게임오버
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

            string waitCharacter = "독백,선택지";
            if (waitCharacter.Contains(currentChatData.character)) isWait = true;
            //if (currentScene != SCENE.INGAME && currentChatData.character == "아트님") isWait = true;
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
        Debug.Log("게임데이터 리셋");

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

    // 현재 진행 흐름에 맞는 대화인지
    public bool IsRight(int index)
    {
        if (selectedNum < 0) return true;
        // 배드엔딩 아닌경우 배드엔딩 채팅 범위에 속하지 않아야 함
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

    public void ChangeChatroom(int index)
    {
        if (index == -1) chatroom = null;
        else chatroom = DataManager.Instance.chatroomList[index];
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
            if (chatListUI == null) chatListUI = GameObject.Find("Canvas").GetComponent<ChatListUI>();
        }

        // 인게임(채팅창)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI == null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
        }

        // 엔딩
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
