using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("수치 설정")]
    [Range(1, 10)]
    [SerializeField] private int speed;   // 채팅 진행 속도

    [SerializeField] private Button goMainBtn;

    private static GameManager instance;

    private List<Text> readCountTxt = new List<Text>();

    private AudioController audioController;
    private ChatListUI chatListUI;
    private ChatUI chatUI;


    // 세이브 시점: 선택지 선택 후, 에피소드 전환, 
    #region SAVE_DATA
    private bool isTutorialFinished = false;                // 튜토리얼 완료 여부(최초 앱 설치시에만 튜토리얼 표시)
    private bool isEpisodeFinished = false;                 // 현재 에피소드 완료 여부(완료 후 채팅 띄우기 위해; 에피소드 전환 시 리셋)
    private int episodeIndex = -1;                          // 현재 에피소드 번호
    private int choiceNum = -1;                              // 현재 에피소드에서 마지막으로 완료한 선택지 번호
    private int currentChatIndex = 0;                        // 현재 출력할 채팅 번호(세이브포인트: 선택지 이후)
    private ENDING ending = ENDING.NORMAL;                   // 엔딩

    private List<bool> isChoiceRight = new List<bool>();     // 각 선택지에 대한 선택이 정답인지(에피소드 전환 시 리셋)
    #endregion --------------------------------------------------------------------------------------------------

    private string chatroom = null;                         // 현재 입장한 채팅방 이름
    private SCENE currentScene;                             // 현재 씬 타입


    public List<int> mainEpisodeCnt;

    public enum ENDING { NORMAL, BAD1, BAD2, BAD3, }
    public enum SCENE { MAIN, CHATLIST, INGAME, ENDING, }

    #region 프로퍼티

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
        set { isEpisodeFinished = value; }
    }
    public List<bool> IsChoiceRight
    {
        get { return isChoiceRight; }
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
        set { currentChatIndex = value; }
    }

    public ENDING Ending
    {
        get { return ending; }
    }

    public string Chatroom
    {
        get { return chatroom; }
        set { chatroom = value; }
    }
    #endregion 프로퍼티 -----------------------------------------------------------------------------------

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
        // 터치음
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

    public void GoToNextEpisode()
    {
        ChangeScene(SCENE.CHATLIST);
        episodeIndex++;
        currentChatIndex = DataManager.Instance.GetStartIndex(episodeIndex);
        choiceNum = -1;
        isEpisodeFinished = false;
        ChatListManager.Instance.ResetState();
        audioController.SetNextIngameBGM(episodeIndex);
        isChoiceRight.Clear();
        Save();
    }

    public void Save()
    {
        int tutorialState = (isTutorialFinished)? 1 : 0;
        int episodeState = (isEpisodeFinished) ? 1 : 0;
        int choiceResult = 0;
        for(int i=0; i<isChoiceRight.Count; i++)
        {
            choiceResult <<= 1;
            if (isChoiceRight[i]) choiceResult++;
        }
        PlayerPrefs.SetInt("IsTutorialFinished", tutorialState);
        PlayerPrefs.SetInt("IsEpisodeFinished", episodeState);
        PlayerPrefs.SetInt("IsChoiceRight", choiceResult);

        PlayerPrefs.SetInt("Episode", episodeIndex);
        PlayerPrefs.SetInt("ChoiceNum", choiceNum);
        PlayerPrefs.SetInt("ChatIndex", currentChatIndex);
        PlayerPrefs.SetInt("Ending", (int)ending);

        ChatListManager.Instance.Save();

        //Debug.Log(
            //"[saved]" +
            //" epiFinished:" + isEpisodeFinished +
            //", currentChatIndex:" + currentChatIndex +
            //", episodeIndex:" + episodeIndex +
            //", choiceNum:" + choiceNum +
            //", ending:" + ending);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("Episode"))
        {
            isTutorialFinished = (PlayerPrefs.GetInt("IsTutorialFinished") == 1);
            isEpisodeFinished = (PlayerPrefs.GetInt("IsEpisodeFinished") == 1);

            episodeIndex = PlayerPrefs.GetInt("Episode");
            choiceNum = PlayerPrefs.GetInt("ChoiceNum");

            isChoiceRight = new List<bool>();
            int sum = PlayerPrefs.GetInt("IsChoiceRight");
            //Debug.Log("sum:" + sum);
            for (int i=0; i<=choiceNum; i++)
            {
                isChoiceRight.Add(sum%2==1);
                sum >>= 1;
            }
            isChoiceRight.Reverse();

            currentChatIndex = PlayerPrefs.GetInt("ChatIndex");
            ending = (ENDING)PlayerPrefs.GetInt("Ending");

            // 각 채팅방의 읽음 상태 불러오기
            for (int i = 0; i < DataManager.Instance.chatroomList.Count; i++)
            {
                bool value = (PlayerPrefs.GetInt("IsEntered_" + i) == 1);
                ChatListManager.Instance.SetIsEntered(i, value);
            }

            audioController.LoadIngameBGM();

            // 로드 완료--------------------------------------------------------------

            //Debug.Log(
                //"[Loaded]" +
                //" epiFinished:"+ isEpisodeFinished +
                //", currentChatIndex:" + currentChatIndex +
                //", episodeIndex:" + episodeIndex +
                //", choiceNum:"+ choiceNum +
                //", ending:" + ending);
        }
    }

    public void ResetGame()
    {
        //Debug.Log("게임데이터 리셋");

        // PlayerPrefs 데이터 삭제(튜토리얼 완료 상태, 음량 설정은 유지)
        //if (!PlayerPrefs.HasKey("IsTutorialFinished")) isTutorialFinished = false;
        isTutorialFinished = (PlayerPrefs.GetInt("IsTutorialFinished") == 1);
        float bgmVolume = audioController.GetBgmVolume();
        float effectVolume = audioController.GetEffectVolume();
        PlayerPrefs.DeleteAll();
        audioController.SetBgmVolume(bgmVolume);
        audioController.SetEffectVolume(effectVolume);

        isEpisodeFinished = false;
        episodeIndex = -1;
        choiceNum = -1;
        currentChatIndex = 0;
        isChoiceRight.Clear();
        ending = ENDING.NORMAL;

        ChatListManager.Instance.ResetState();
    }

    public void FinishTutorial()
    {
        isTutorialFinished = true;
        GoToNextEpisode();
    }

    public void AddReadCountTxt(Text text)
    {
        readCountTxt.Add(text);
    }

    public void AddChoiceResult(bool value)
    {
        isChoiceRight.Add(value);
    }

    public void ClearReadCount()
    {
        readCountTxt.Clear();
    }

    // 오답 선택지 선택 시 현재 에피소드 번호를 엔딩번호로 지정
    public void ChangeEnding()
    {
        if (ending == (ENDING)(episodeIndex + 1)) return;
        //Debug.Log("엔딩 결정됨");
        ending = (ENDING)(episodeIndex+1);
        audioController.ChangeMood(episodeIndex);
    }

    public void ChangeScene(SCENE scene)
    {
        //audioController.StopBGM();
        SceneManager.LoadScene((int)scene);
    }

    // 씬 로드 시 호출
    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log(scene.name + " 씬 로드됨");

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
            audioController.PlayBGM(AudioController.BGM.MAIN);
        }

        // 인게임(채팅창)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            currentScene = SCENE.INGAME;
            if (chatUI == null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
            if (chatroom.Equals(Constants.grouptalk)) audioController.PlayIngameBGM();
            else if(!chatroom.Equals(Constants.mastertalk))
            {
                string bgmName = "EP" + (episodeIndex + 1);
                audioController.PlayBGM((AudioController.BGM)System.Enum.Parse(typeof(AudioController.BGM), bgmName));
            }
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
