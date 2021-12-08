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





    // ��ȿ����
    [SerializeField]
    private GameObject[] readIcon;
    private Text[] readTxt;
    private bool[] isEntered;






    ChatData chatData;

    private void Awake()
    {
        Debug.Log("���� �������");

        GameObject content = scrollView.transform.GetChild(0).GetChild(0).gameObject;
        int size = content.transform.childCount;
        chatListBlock = new GameObject[size];
        chatListBtn = new Button[size];
        nameTxt = new Text[size];
        previewTxt = new Text[size];
        timeTxt = new Text[size];
        dateTxt = new Text[size];

        



        //��ȿ����
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




            //��ȿ����
            readIcon[i] = chatListBlock[i].transform.GetChild(0).transform.GetChild(0).gameObject;
            readTxt[i] = readIcon[i].transform.GetChild(0).GetComponent<Text>();
            isEntered[i] = ChatListManager.Instance.GetIsEntered(i);
            
            readIcon[i].SetActive(isEntered[i]);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < chatListBlock.Length; i++)
        {
            chatListBlock[i].SetActive(false);
        }

        int episodeIndex = GameManager.Instance.GetEpisodeIndex();

        // ���� ���Ǽҵ��� ���� ä�ù� �� ����� ����
        if (episodeIndex >= 0)
        {
            List<ChatData> data = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.SUB);
            for (int i = 0; i < data.Count; i++)
            {
                int index = DataManager.Instance.chatroomList.IndexOf(data[i].chatroom);
                SetChatroomBlock(index, data[i]);
            }

            chatData = DataManager.Instance.GetChatData(episodeIndex, GameManager.Instance.currentChatIndex);
            SetChatroomBlock(0, chatData);
        }

        // Ʃ�丮���� �׻� ����
        SetChatroomBlock(5, DataManager.Instance.GetLastTutorial());
    }

    //private void Update()
    //{
    //    int episodeIndex = GameManager.Instance.GetEpisodeIndex();
    //    if (episodeIndex >= 0)
    //    {
    //        chatListBlock[0].SetActive(true);
    //        int recentIndex = GameManager.Instance.currentChatIndex;
    //        if (GameManager.Instance.lastListIndex != recentIndex)
    //        {
    //            for (int i = GameManager.Instance.lastListIndex; i <= recentIndex; i++)
    //            {
    //                chatData = DataManager.Instance.GetChatData(episodeIndex, recentIndex);
    //                if (SetGroupTalkUI())
    //                {
    //                    Debug.Log(i + "��°�� ����Ʈ ����");
    //                    GameManager.Instance.lastListIndex = i;
    //                }
    //            }
    //        }
    //    }
    //}

    //public bool SetGroupTalkUI()
    //{
    //    // ��ǳ�� ���� �������϶��� ȭ�� ����
    //    int characterIndex = DataManager.Instance.characterList.IndexOf(chatData.character);
    //    if (characterIndex > 6 || characterIndex == 3)
    //    {
    //        // ���Ǽҵ� 1������ ī�� �˸��� 
    //        //if (episodeIndex == 0 || chatroomIndex != 0) GameManager.Instance.PlayAnotherAudio(GameManager.AUDIO.NOTIFICATION);
    //        SetChatroomBlock(0, chatData);
    //        return true;
    //    }
    //    else return false;
    //};

    // �ϳ��� ä�ù� �� ����
    private void SetChatroomBlock(int index, ChatData lastData)
    {
        chatListBlock[index].SetActive(true);
        nameTxt[index].text = lastData.character;
        previewTxt[index].text = lastData.text;
        timeTxt[index].text = lastData.time;
        dateTxt[index].text = lastData.date;
        readIcon[index].SetActive(!isEntered[index]);
        Debug.Log("������ ������ ���� : " + isEntered[index]);
    }

    private void EnterChatroom(int index)
    {
        Debug.Log("�ε��� : " + index);
        ChatListManager.Instance.SetIsEntered(index, true);
        Debug.Log("������ ������ ����");
        GameManager.Instance.ChangeChatroom(index);
        GameManager.Instance.ChangeScene(GameManager.SCENE.INGAME);
    }
}
