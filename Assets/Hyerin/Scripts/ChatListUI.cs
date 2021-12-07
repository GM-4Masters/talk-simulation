using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatListUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;

    private Button[] chatListBtn;
    private Text[] nameTxt, previewTxt, timeTxt, dateTxt;

    ChatData chatData;

    private void Awake()
    {
        GameObject content = scrollView.transform.GetChild(0).GetChild(0).gameObject;
        chatListBtn = new Button[content.transform.childCount];
        nameTxt = new Text[content.transform.childCount];
        previewTxt = new Text[content.transform.childCount];
        timeTxt = new Text[content.transform.childCount];
        dateTxt = new Text[content.transform.childCount];
        for (int i=0; i<chatListBtn.Length; i++)
        {
            GameObject chatList = content.transform.GetChild(i).gameObject;
            chatListBtn[i] = chatList.GetComponent<Button>();
            if (i != 0) chatListBtn[i].interactable = false;
            int index = i;
            chatListBtn[i].onClick.AddListener(() => { EnterChatroom(index); });

            nameTxt[i] = chatList.transform.GetChild(1).GetComponent<Text>();
            previewTxt[i] = chatList.transform.GetChild(2).GetComponent<Text>();
            timeTxt[i] = chatList.transform.GetChild(3).GetComponent<Text>();
            dateTxt[i] = chatList.transform.GetChild(4).GetComponent<Text>();
        }
    }

    private void OnEnable()
    {
        if(chatData!=null) Play();
    }

    private void Update()
    {
        int recentIndex = GameManager.Instance.currentChatIndex;
        if (GameManager.Instance.lastListIndex != recentIndex)
        {
            for (int i = GameManager.Instance.lastListIndex + 1; i < recentIndex; i++)
            {
                chatData = DataManager.Instance.GetChatData(0, i);
                Play();
            }
            GameManager.Instance.lastListIndex = recentIndex;
        }
    }

    public void Play()
    {
        // 말풍선 형태 데이터일때만 화면 갱신
        int characterIndex = DataManager.Instance.characterList.IndexOf(chatData.character);
        if (characterIndex > 6 || characterIndex == 3)
        {
            // 에피소드 1에서만 카톡 알림음 
            //if (episodeIndex == 0 || chatroomIndex != 0) GameManager.Instance.PlayAnotherAudio(GameManager.AUDIO.NOTIFICATION);
            Debug.Log("미리보기 갱신:" + chatData.index +"ch:" +chatData.character);
            RefreshChat();
        }
    }

    public void RefreshChat()
    {
        nameTxt[0].text = chatData.character;
        previewTxt[0].text = chatData.text;
        timeTxt[0].text = chatData.time;
        dateTxt[0].text = chatData.date;
    }

    private void EnterChatroom(int index)
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.INGAME);
    }
}
