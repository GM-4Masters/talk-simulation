using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatListUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;
    [SerializeField] private Image groupTalkEmoji; // 단톡방 이모지
    [SerializeField] private Image alertIcon;    // 채팅목록 상단 알림 아이콘

    private GameObject[] chatListBlock;
    private Button[] chatListBtn;
    private Text[] nameTxt, previewTxt, timeTxt, dateTxt;





    // 선효수정
    [SerializeField]
    private GameObject[] readIcon;
    private Text[] readTxt;
    private bool[] isEntered;






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






        //선효수정
        readIcon = new GameObject[size];
        readTxt = new Text[size];
        isEntered = new bool[size];





        for (int i = 0; i < chatListBtn.Length; i++)
        {
            chatListBlock[i] = content.transform.GetChild(i).gameObject;
            chatListBtn[i] = chatListBlock[i].GetComponent<Button>();
            int index = i;
            chatListBtn[i].onClick.AddListener(() => { EnterChatroom(index); });

            nameTxt[i] = chatListBlock[i].transform.GetChild(1).GetComponent<Text>();
            previewTxt[i] = chatListBlock[i].transform.GetChild(2).GetComponent<Text>();
            timeTxt[i] = chatListBlock[i].transform.GetChild(3).GetComponent<Text>();
            dateTxt[i] = chatListBlock[i].transform.GetChild(4).GetComponent<Text>();




            ////선효수정
            readIcon[i] = chatListBlock[i].transform.GetChild(0).transform.GetChild(0).gameObject;
            ////readTxt[i] = readIcon[i].transform.GetChild(0).GetComponent<Text>();
            //isEntered[i] = ChatListManager.Instance.GetIsEntered(i);

            //readIcon[i].SetActive(isEntered[i]);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < chatListBtn.Length; i++)
        {
            isEntered[i] = ChatListManager.Instance.GetIsEntered(i);

            readIcon[i].SetActive(isEntered[i]);
        }

        int episodeIndex = GameManager.Instance.GetEpisodeIndex();

        // 단톡방 이모지, 채팅방 아이콘 세팅
        groupTalkEmoji.sprite = Resources.Load<Sprite>("Sprites/Emoji/"+ Mathf.Clamp(episodeIndex+1,0,5));
        groupTalkEmoji.SetNativeSize();
        string offState = ((episodeIndex >= 1) ? " OFF" : "");
        alertIcon.sprite = Resources.Load<Sprite>("Sprites/CHAT/ALERT"+offState);

        for (int i = 0; i < chatListBlock.Length; i++)
        {
            chatListBlock[i].SetActive(false);
        }

        // 현재 에피소드의 서브 채팅방 및 단톡방 세팅
        if (episodeIndex >= 0)
        {
            // 서브 채팅방
            List<ChatData> data = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.SUB);
            for (int i = 0; i < data.Count; i++)
            {
                int index = DataManager.Instance.chatroomList.IndexOf(data[i].chatroom);
                SetChatroomBlock(index, data[i]);
            }

            // 단톡방
            timeTxt[0].text = "";
            dateTxt[0].text = "";
            previewTxt[0].text = "";
            StartCoroutine(LateUpdateCrt(episodeIndex));

            SetChatroomBlock(5, DataManager.Instance.GetLastTutorial());
        }
        else
        {
            SetChatroomBlock(5, DataManager.Instance.GetFirstTutorial());
        }
    }

    // 늦은 업데이트
    private IEnumerator LateUpdateCrt(int episodeIndex)
    {
        yield return new WaitForSeconds(0.05f);
        // 게임시작 전 시점에는 가장 최근 메시지, 에피소드 끝난 후에는 가장 최근 사람 메시지,
        // 나머지 경우(에피소드 진행 중 강종 후 재시작)에는 세이브포인트 시점 메시지를 출력
        if (GameManager.Instance.choiceNum == -1) chatData = DataManager.Instance.GetChatData(episodeIndex,GameManager.Instance.currentChatIndex-1);
        else if (GameManager.Instance.IsEpisodeFinished()) chatData = DataManager.Instance.GetLastChat(GameManager.Instance.ending == GameManager.ENDING.NORMAL, episodeIndex);
        else chatData = DataManager.Instance.GetChatData(episodeIndex, GameManager.Instance.lastChatIndex + 1);
        SetChatroomBlock(0, chatData);


        // 에피소드 끝났다면 갠톡 세팅
        chatListBlock[0].GetComponent<Button>().interactable = !GameManager.Instance.IsEpisodeFinished();
        if (GameManager.Instance.IsEpisodeFinished())
        {
            List<ChatData> personalChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
            //Debug.Log("personalchat[0]:" + personalChat[0].chatroom);
            int index = DataManager.Instance.chatroomList.IndexOf(personalChat[0].chatroom);
            SetChatroomBlock(index, personalChat[personalChat.Count - 4]);
        }
    }

    //private void Update()
    //{
    //    int episodeIndex = GameManager.Instance.GetEpisodeIndex();
    //    if (episodeIndex >= 0)
    //    {
    //        chatListBlock[0].SetActive(true);
    //        int recentIndex = GameManager.Instance.currentChatIndex;

    //        if (GameManager.Instance.lastChatIndex != recentIndex)
    //        {
    //            for (int i = GameManager.Instance.lastChatIndex; i <= recentIndex; i++)
    //            {
    //                chatData = DataManager.Instance.GetChatData(episodeIndex, recentIndex);
    //                SetGroupTalkUI();
    //            }
    //        }
    //    }
    //}

    public void SetGroupTalkUI()
    {
        // 말풍선 형태 데이터일때만 화면 갱신
        int characterIndex = DataManager.Instance.characterList.IndexOf(chatData.character);
        if (characterIndex > 6 || characterIndex == 3)
        {
            // 에피소드 1에서만 카톡 알림음 
            //if (episodeIndex == 0 || chatroomIndex != 0) GameManager.Instance.PlayAnotherAudio(GameManager.AUDIO.NOTIFICATION);
            SetChatroomBlock(0, chatData);
        }
    }

    // 하나의 채팅방 블럭 세팅
    private void SetChatroomBlock(int index, ChatData lastData)
    {
        chatListBlock[index].SetActive(true);
        previewTxt[index].text = lastData.text;
        timeTxt[index].text = lastData.time;
        dateTxt[index].text = lastData.date;
        readIcon[index].SetActive(!isEntered[index]);
        //Debug.Log("안읽음 아이콘 상태 : " + isEntered[index]);
    }

    private void EnterChatroom(int index)
    {
        //Debug.Log("인덱스 : " + index);
        ChatListManager.Instance.SetIsEntered(index, true);
        ChatListManager.Instance.Save();
        //Debug.Log("안읽음 아이콘 제거");
        GameManager.Instance.ChangeChatroom(index);
        GameManager.Instance.ChangeScene(GameManager.SCENE.INGAME);
    }
}
