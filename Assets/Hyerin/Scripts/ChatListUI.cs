using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatListUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;

    private Button[] chatListBtn;
    private Text[] nameTxt, previewTxt, timeTxt, dateTxt;

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

    public void RefreshChat(ChatData td)
    {
        nameTxt[0].text = td.GetName();
        previewTxt[0].text = td.GetText();
        timeTxt[0].text = td.GetTime().ToString();  // 시간 형식에 맞게 바꾸어 출력할 것
        dateTxt[0].text = td.GetDate().ToString();  // 날짜 형식에 맞게 바꾸어 출력할 것
    }

    private void EnterChatroom(int index)
    {
        GameManager.Instance.SetRoomIndex(index);
        GameManager.Instance.ChangeScene(GameManager.SCENE.INGAME);
    }
}
