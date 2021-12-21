using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System;

public class ChatListUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;
    [SerializeField] private Image groupTalkEmoji; // 단톡방 이모지
    [SerializeField] private Image alertIcon;    // 채팅목록 상단 알림 아이콘
    [SerializeField] private GameObject popupMessage;   // 팝업메시지(입장 불가 등)

    private GameObject[] chatListBlock;
    private Button[] chatListBtn;
    private Image[] chatProfileImg;
    private Text[] nameTxt, previewTxt, timeTxt, dateTxt;

    private PlayableDirector popupMessagePd;



    // 선효수정
    [SerializeField]
    private GameObject[] readIcon;
    //private Text[] readTxt;
    private bool[] isEntered;


    private void Awake()
    {
        GameObject content = scrollView.transform.GetChild(0).GetChild(0).gameObject;
        int size = content.transform.childCount;
        chatListBlock = new GameObject[size];
        chatListBtn = new Button[size];
        chatProfileImg = new Image[size];
        nameTxt = new Text[size];
        previewTxt = new Text[size];
        timeTxt = new Text[size];
        dateTxt = new Text[size];

        popupMessagePd = popupMessage.GetComponent<PlayableDirector>();

        //선효수정--------------------------
        readIcon = new GameObject[size];
        //readTxt = new Text[size];
        isEntered = new bool[size];
        //선효수정END-----------------------


        for (int i = 0; i < chatListBtn.Length; i++)
        {
            chatListBlock[i] = content.transform.GetChild(i).gameObject;
            chatListBtn[i] = chatListBlock[i].GetComponent<Button>();

            chatProfileImg[i] = chatListBlock[i].transform.GetChild(0).GetComponent<Image>();
            nameTxt[i] = chatListBlock[i].transform.GetChild(1).GetComponent<Text>();
            previewTxt[i] = chatListBlock[i].transform.GetChild(2).GetComponent<Text>();
            timeTxt[i] = chatListBlock[i].transform.GetChild(3).GetComponent<Text>();
            dateTxt[i] = chatListBlock[i].transform.GetChild(4).GetComponent<Text>();

            //선효수정--------------------------
            readIcon[i] = chatListBlock[i].transform.GetChild(0).transform.GetChild(0).gameObject;
            //선효수정END-----------------------
        }
    }

    private void OnEnable()
    {
        int episodeIndex = GameManager.Instance.EpisodeIndex;
        int lastChatIndex = GameManager.Instance.CurrentChatIndex;

        if (episodeIndex == -1 && GameManager.Instance.IsTutorialFinished) GameManager.Instance.GoToNextEpisode();

        // 알림표시(빨간원) 설정
        for (int i = 0; i < chatListBtn.Length; i++)
        {
            isEntered[i] = ChatListManager.Instance.GetIsEntered(i);

            readIcon[i].SetActive(isEntered[i]);
        }

        // 에피소드 2부터는 알림 꺼짐표시(우측 상단)
        string offState = ((episodeIndex >= 1) ? " OFF" : "");
        alertIcon.sprite = Resources.Load<Sprite>("Sprites/CHAT/ALERT" + offState);



        // 현재 에피소드에 해당하는 채팅방 세팅(-1 : 튜토리얼)
        int blockIndex = 0;
        if (episodeIndex < 0)
        {
            SetChatroomBlock(blockIndex++, DataManager.Instance.GetFirstTutorial());
            groupTalkEmoji.enabled = false;
        }
        else
        {
            // 그룹채팅방(메인) 및 튜토리얼 채팅방
            SetChatroomBlock(blockIndex++, DataManager.Instance.GetMainChat(episodeIndex, lastChatIndex));
            SetChatroomBlock(blockIndex++, DataManager.Instance.GetLastTutorial());

            // 그룹채팅방 이모지 설정
            groupTalkEmoji.enabled = true;
            groupTalkEmoji.sprite = Resources.Load<Sprite>("Sprites/Emoji/" + Mathf.Clamp(episodeIndex + 1, 0, 5));
            groupTalkEmoji.SetNativeSize();

            // 에피소드 종료 여부에 따라 그룹채팅방 활성화 상태 결정
            bool isEpisodeFinished = GameManager.Instance.IsEpisodeFinished;

            // 에피소드가 끝났다면 개인채팅방 세팅(DataManager에서)
            if (isEpisodeFinished)
            {
                List<ChatData> personalChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
                SetChatroomBlock(blockIndex++, personalChat[personalChat.Count - 4]);
            }

            // 서브채팅방
            // (DataManager에서 각 채팅방의 마지막 채팅만 가져오기)
            List<ChatData> subList = DataManager.Instance.GetLastSubChat(episodeIndex);
            for(int i=0; i<subList.Count; i++)
            {
                SetChatroomBlock(blockIndex++, subList[i]);
            }
        }

        // 이하 채팅방 블럭 비활성화
        for (int i = blockIndex; i < chatListBlock.Length; i++)
        {
            chatListBlock[i].SetActive(false);
        }

    }

    // 하나의 채팅방 블럭 세팅
    private void SetChatroomBlock(int index, ChatData lastData)
    {
        chatListBlock[index].SetActive(true);
        chatProfileImg[index].sprite = Resources.Load<Sprite>("Sprites/Profile/" + lastData.chatroom);
        if (lastData.chatroom.Equals(Constants.grouptalk)) nameTxt[index].text = Constants.grouptalkName;
        else nameTxt[index].text = lastData.chatroom;
        previewTxt[index].text = lastData.text;
        timeTxt[index].text = lastData.time;
        dateTxt[index].text = lastData.date;
        readIcon[index].SetActive(!isEntered[index]);
         
        chatListBtn[index].onClick.AddListener(() => { EnterChatroom(index, lastData.chatroom); });
    }

    // 채팅방 입장(각 채팅방 블럭 터치 시 호출)
    private void EnterChatroom(int index, string chatroom)
    {
        //// 에피소드 종료 후 그룹채팅방 입장시 입장불가 팝업 띄우기
        //if (GameManager.Instance.IsEpisodeFinished && index==0)
        //{
        //    popupMessagePd.Play();
        //    return;
        //}
        
        ChatListManager.Instance.SetIsEntered(index, true);
        ChatListManager.Instance.Save();

        GameManager.Instance.Chatroom = chatroom;
        GameManager.Instance.ChangeScene(GameManager.SCENE.INGAME);
    }
}
