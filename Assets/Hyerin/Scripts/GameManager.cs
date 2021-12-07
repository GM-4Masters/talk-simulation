using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private ChatData currentTextData;       // ����� ä�� ������

    //private bool isPlay;                    // �ΰ��� �ð��帧 ����
    private float spendTime;                // �帥 �ð�(ä�� ��� �� ����)
    private int roomIndex;                  // ������ ä�ù� ��ȣ
    private int currentTextIndex;           // ���� ����� ä�� ��ȣ
    private int episodeIndex;               // ���� ���Ǽҵ� ��ȣ
    private SCENE currentScene;             // ���� �� Ÿ��
    public ENDING ending;                   // ����

    public int[] boundary = { 30, 60, 90 }; // ���Ǽҵ� �б���

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

        // ���� ���Ǽҵ� ��� �Ǵ� ���ӿ���
        if (currentTextIndex == boundary[episodeIndex])
        {
            if (ending != ENDING.NORMAL) ShowEnding();
            else episodeIndex++;
        }
        //if (spendTime < currentTextData.dt)
        //{
        //    // ���� ä�� ���
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

    // �ð� ���
    public void Play()
    {
        //isPlay = true;
        Time.timeScale = 1f;
    }

    // ������ ������ ������ ���
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

    // �������� ���� �� ���� ���Ǽҵ� ��ȣ�� ������ȣ�� ����
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

    // �� �ε� �� ȣ��
    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " �� �ε��");

        if (scene.buildIndex == (int)SCENE.MAIN)
        {
            PlayAudio(AUDIO.MAIN);
        }
        
        if (scene.buildIndex == (int)SCENE.CHATLIST)
        {
            if (chatListUI == null)  chatListUI = GameObject.Find("Canvas").GetComponent<ChatListUI>();
        }

        // �ΰ���(ä��â)
        if (scene.buildIndex == (int)SCENE.INGAME)
        {
            if(chatUI==null) chatUI = GameObject.Find("Canvas").GetComponent<ChatUI>();
        }

        // ����
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
