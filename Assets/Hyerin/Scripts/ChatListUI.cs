using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatListUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;

    private GameObject[] chatListBlock;
    private Button[] chatListBtn;
    private Text[] nameTxt, previewTxt, timeTxt, dateTxt;

    ChatData chatData;

    private void Awake()
    {
        GameObject content = scrollView.transform.GetChild(0).GetChild(0).gameObject;
        int size = content.transform.childCount;
        chatListBlock = new GameObject[size];
        chatListBtn = new Button[size];
        nameTxt = new Text[size];
        previewTxt = new Text[size];
        timeTxt = new Text[size];
        dateTxt = new Text[size];
        for (int i=0; i<chatListBtn.Length; i++)
        {
            chatListBlock[i] = content.transform.GetChild(i).gameObject;
            chatListBtn[i] = chatListBlock[i].GetComponent<Button>();
            int index = i;
            chatListBtn[i].onClick.AddListener(() => { EnterChatroom(index); });

            nameTxt[i] = chatListBlock[i].transform.GetChild(1).GetComponent<Text>();
            previewTxt[i] = chatListBlock[i].transform.GetChild(2).GetComponent<Text>();
            timeTxt[i] = chatListBlock[i].transform.GetChild(3).GetComponent<Text>();
            dateTxt[i] = chatListBlock[i].transform.GetChild(4).GetComponent<Text>();
        }
    }

    private void OnEnable()
    {
        for(int i=0; i<chatListBlock.Length; i++)
        {
            chatListBlock[i].SetActive(false);
        }

        int episodeIndex = GameManager.Instance.GetEpisodeIndex();

        // 현재 에피소드의 서브 채팅방 및 단톡방 세팅
        if (episodeIndex >= 0)
        {
            List<ChatData> data = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.SUB);
            for (int i = 0; i < data.Count; i++)
            {
                int index = DataManager.Instance.chatroomList.IndexOf(data[i].chatroom);
                SetChatroomBlock(index, data[i]);
            }

            Debug.Log("lastlist:"+GameManager.Instance.lastListIndex);
            chatData = DataManager.Instance.GetChatData(episodeIndex, GameManager.Instance.lastListIndex);
            SetGroupTalkUI();
        }

        // 튜토리얼은 항상 보임
        SetChatroomBlock(5, DataManager.Instance.GetLastTutorial());
    }

    private void Update()
    {
        int episodeIndex = GameManager.Instance.GetEpisodeIndex();
        if(episodeIndex >= 0)
        {
            chatListBlock[0].SetActive(true);
            int recentIndex = GameManager.Instance.currentChatIndex;
            if (GameManager.Instance.lastListIndex != recentIndex)
            {
                chatData = DataManager.Instance.GetChatData(episodeIndex, recentIndex);
                SetGroupTalkUI();
            }
        }
    }
    public void SetGroupTalkUI()
    {
        // 말풍선 형태 데이터일때만 화면 갱신
        int characterIndex = DataManager.Instance.characterList.IndexOf(chatData.character);
        if (characterIndex > 6 || characterIndex == 3)
        {
            // 에피소드 1에서만 카톡 알림음 
            //if (episodeIndex == 0 || chatroomIndex != 0) GameManager.Instance.PlayAnotherAudio(GameManager.AUDIO.NOTIFICATION);
            SetChatroomBlock(0, chatData);
            GameManager.Instance.lastListIndex = GameManager.Instance.currentChatIndex;
        }
    }

    // 하나의 채팅방 블럭 세팅
    private void SetChatroomBlock(int index, ChatData lastData)
    {
        chatListBlock[index].SetActive(true);
        nameTxt[index].text = lastData.character;
        previewTxt[index].text = lastData.text;
        timeTxt[index].text = lastData.time;
        dateTxt[index].text = lastData.date;
    }

    private void EnterChatroom(int index)
    {
        GameManager.Instance.ChangeChatroom(index);
        GameManager.Instance.ChangeScene(GameManager.SCENE.INGAME);
    }
}
