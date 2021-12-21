using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System;

public class ChatListUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;
    [SerializeField] private Image groupTalkEmoji; // ����� �̸���
    [SerializeField] private Image alertIcon;    // ä�ø�� ��� �˸� ������
    [SerializeField] private GameObject popupMessage;   // �˾��޽���(���� �Ұ� ��)

    private GameObject[] chatListBlock;
    private Button[] chatListBtn;
    private Image[] chatProfileImg;
    private Text[] nameTxt, previewTxt, timeTxt, dateTxt;

    private PlayableDirector popupMessagePd;



    // ��ȿ����
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

        //��ȿ����--------------------------
        readIcon = new GameObject[size];
        //readTxt = new Text[size];
        isEntered = new bool[size];
        //��ȿ����END-----------------------


        for (int i = 0; i < chatListBtn.Length; i++)
        {
            chatListBlock[i] = content.transform.GetChild(i).gameObject;
            chatListBtn[i] = chatListBlock[i].GetComponent<Button>();

            chatProfileImg[i] = chatListBlock[i].transform.GetChild(0).GetComponent<Image>();
            nameTxt[i] = chatListBlock[i].transform.GetChild(1).GetComponent<Text>();
            previewTxt[i] = chatListBlock[i].transform.GetChild(2).GetComponent<Text>();
            timeTxt[i] = chatListBlock[i].transform.GetChild(3).GetComponent<Text>();
            dateTxt[i] = chatListBlock[i].transform.GetChild(4).GetComponent<Text>();

            //��ȿ����--------------------------
            readIcon[i] = chatListBlock[i].transform.GetChild(0).transform.GetChild(0).gameObject;
            //��ȿ����END-----------------------
        }
    }

    private void OnEnable()
    {
        int episodeIndex = GameManager.Instance.EpisodeIndex;
        int lastChatIndex = GameManager.Instance.CurrentChatIndex;

        if (episodeIndex == -1 && GameManager.Instance.IsTutorialFinished) GameManager.Instance.GoToNextEpisode();

        // �˸�ǥ��(������) ����
        for (int i = 0; i < chatListBtn.Length; i++)
        {
            isEntered[i] = ChatListManager.Instance.GetIsEntered(i);

            readIcon[i].SetActive(isEntered[i]);
        }

        // ���Ǽҵ� 2���ʹ� �˸� ����ǥ��(���� ���)
        string offState = ((episodeIndex >= 1) ? " OFF" : "");
        alertIcon.sprite = Resources.Load<Sprite>("Sprites/CHAT/ALERT" + offState);



        // ���� ���Ǽҵ忡 �ش��ϴ� ä�ù� ����(-1 : Ʃ�丮��)
        int blockIndex = 0;
        if (episodeIndex < 0)
        {
            SetChatroomBlock(blockIndex++, DataManager.Instance.GetFirstTutorial());
            groupTalkEmoji.enabled = false;
        }
        else
        {
            // �׷�ä�ù�(����) �� Ʃ�丮�� ä�ù�
            SetChatroomBlock(blockIndex++, DataManager.Instance.GetMainChat(episodeIndex, lastChatIndex));
            SetChatroomBlock(blockIndex++, DataManager.Instance.GetLastTutorial());

            // �׷�ä�ù� �̸��� ����
            groupTalkEmoji.enabled = true;
            groupTalkEmoji.sprite = Resources.Load<Sprite>("Sprites/Emoji/" + Mathf.Clamp(episodeIndex + 1, 0, 5));
            groupTalkEmoji.SetNativeSize();

            // ���Ǽҵ� ���� ���ο� ���� �׷�ä�ù� Ȱ��ȭ ���� ����
            bool isEpisodeFinished = GameManager.Instance.IsEpisodeFinished;

            // ���Ǽҵ尡 �����ٸ� ����ä�ù� ����(DataManager����)
            if (isEpisodeFinished)
            {
                List<ChatData> personalChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
                SetChatroomBlock(blockIndex++, personalChat[personalChat.Count - 4]);
            }

            // ����ä�ù�
            // (DataManager���� �� ä�ù��� ������ ä�ø� ��������)
            List<ChatData> subList = DataManager.Instance.GetLastSubChat(episodeIndex);
            for(int i=0; i<subList.Count; i++)
            {
                SetChatroomBlock(blockIndex++, subList[i]);
            }
        }

        // ���� ä�ù� �� ��Ȱ��ȭ
        for (int i = blockIndex; i < chatListBlock.Length; i++)
        {
            chatListBlock[i].SetActive(false);
        }

    }

    // �ϳ��� ä�ù� �� ����
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

    // ä�ù� ����(�� ä�ù� �� ��ġ �� ȣ��)
    private void EnterChatroom(int index, string chatroom)
    {
        //// ���Ǽҵ� ���� �� �׷�ä�ù� ����� ����Ұ� �˾� ����
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
