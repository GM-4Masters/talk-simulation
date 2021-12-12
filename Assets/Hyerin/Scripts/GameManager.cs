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
    public int speed;   // 채팅 진행 속도

    [SerializeField] private Button goMainBtn;

    private static GameManager instance;

    private List<Text> readCountTxt = new List<Text>();

    private AudioController audioController;
    private ChatListUI chatListUI;
    private ChatUI chatUI;


    // 세이브 시점: 선택지 선택 후, 에피소드 전환, 
    #region SAVE_DATA ------------------------------------------------------------------------------------------
    private bool isTutorialFinished = false;                // 튜토리얼 완료 여부
    private bool isEpisodeFinished = false;                 // 현재 에피소드 완료 여부(완료 후 채팅 띄우기 위해; 에피소드 전환 시 리셋)
    public bool isChoiceRight;                              // 현재 선택지에 대한 선택이 정답인지
    private int episodeIndex = -1;                          // 현재 에피소드 번호
    public int choiceNum = -1;                              // 현재 에피소드에서 마지막으로 완료한 선택지 번호
    public int currentChatIndex = 0;                        // 현재 출력할 채팅 번호
    public int groupTalkUnChecked = 3;                      // (단톡방 한정)안읽은 사람 수
    public ENDING ending = ENDING.NORMAL;                   // 엔딩
    #endregion --------------------------------------------------------------------------------------------------

    private ChatData currentChatData;                       // 출력할 채팅 데이터(세이브포인트: 선택지 이후)
    private bool isWait = false;                            // 대기상태
    public int lastChatIndex = -1;                          // 가장 최근 갱신된 채팅 번호
    private float spendTime;                                // 흐른 시간(채팅 출력 후 리셋)
    public string chatroom = null;                          // 현재 입장한 채팅방 이름(팀단톡,김산호,벅찬우,이채린,엄마,4MasterTalk,GameMasters)
    private SCENE currentScene;                             // 현재 씬 타입


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

        //(배드엔딩 마지막 또는 에피소드2의 마지막) 채팅 출력이 끝났다면 채팅출력 중단
        if ( (!isEpisodeFinished) &&
             ( (ending != ENDING.NORMAL && DataManager.Instance.IsLastBadEndingChat(currentChatData.index) ) ||
               (episodeIndex == 2 && currentChatIndex >= mainEpisodeCnt[episodeIndex] - 1) ) )
        {
            if (!isEpisodeFinished) isEpisodeFinished = true;
        }
        else if (episodeIndex >= 0 && !isWait && spendTime >= currentChatData.dt)
        {
            // 에피소드 진행중이고
            // 현재 채팅방에 있거나 채팅방에 없어도 실행되는 데이터일 경우
            int characterIndex = DataManager.Instance.characterList.IndexOf(currentChatData.character);
            if (currentChatIndex < mainEpisodeCnt[episodeIndex] &&
                (chatroom == currentChatData.chatroom || (characterIndex > 2 && characterIndex < 13)))
            {
                GoNext();
            }
        }

        // 터치음
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
        // 현재 에피소드 끝까지 진행되었을 경우 - 다음 에피소드로 이동 or 게임오버
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
            // 선택지 지점이면 세이브
            if (DataManager.Instance.IsSavePoint(currentChatData.index, episodeIndex)) Save();

            currentChatIndex++;
            currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
            spendTime = 0f;

            //Debug.Log("GoNext:"+currentChatIndex);
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

            // 각 채팅방의 읽음 상태 불러오기
            for (int i = 0; i < DataManager.Instance.chatroomList.Count; i++)
            {
                bool value = (PlayerPrefs.GetInt("IsEntered_" + i) == 1);
                ChatListManager.Instance.SetIsEntered(i, value);
            }

            audioController.LoadIngameBGM();

            // 로드 완료--------------------------------------------------------------

            // 엔딩에 따라 채팅시작지점 변경
            if (ending != ENDING.NORMAL) currentChatIndex += DataManager.Instance.GetBadEndingOffset(episodeIndex,choiceNum);
            lastChatIndex = currentChatIndex-1;

            currentChatData = DataManager.Instance.GetChatData(episodeIndex, currentChatIndex);
            //Debug.Log("episodeIndex:" + episodeIndex+ ", currentChatIndex:"+ currentChatIndex);
            Debug.Log("Loaded:" + currentChatIndex + ", ending:" + ending);
        }
    }

    public void ResetGame()
    {
        Debug.Log("게임데이터 리셋");

        // PlayerPrefs 데이터 삭제(음량 설정은 유지)
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

    // 오답 선택지 선택 시 현재 에피소드 번호를 엔딩번호로 지정
    public void ChangeEnding()
    {
        if (ending == (ENDING)(episodeIndex + 1)) return;
        Debug.Log("엔딩 결정됨");
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

    // 씬 로드 시 호출
    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " 씬 로드됨");

        // 메뉴창 '메인으로' 버튼 활성화 상태 변경
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

        // 인게임(채팅창)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI == null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
            if(chatroom==DataManager.Instance.chatroomList[0]) audioController.PlayIngameBGM();
        }

        // 엔딩
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
