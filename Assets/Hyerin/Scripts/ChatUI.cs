using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button[] tempEndingBtn = new Button[3];

    private void Awake()
    {
    }

    public void RefreshChat(ChatData td)
    {

    }

    public void ExitChatroom()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }

    // 임시코드
    public void GoEnding(int index)
    {
        GameManager.Instance.ending = (GameManager.ENDING)index;
        GameManager.Instance.ShowEnding();
    }
}
