using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private Button exitBtn;

    public GameObject playerBubblePrefab, npcBubblePrefab;
    public RectTransform contentRect;
    public Scrollbar scrollBar;

    private void Awake()
    {
    }

    public void RefreshChat(ChatData td)
    {
        switch (td.GetMemberIndex())
        {
            case 0:
                // 시스템
                break;
            case 1:
                // 나
                Chat(true, td.GetText(), "");
                break;
            default:
                // 팀원
                Chat(false, td.GetText(), td.GetName());
                break;
        }
    }

    public void Chat(bool isSend, string text, string name)
    {
        BubbleScript Bubble = Instantiate(isSend ? playerBubblePrefab : npcBubblePrefab).GetComponent<BubbleScript>();
        Bubble.transform.SetParent(contentRect.transform, false);
        Bubble.TextRect.GetComponent<Text>().text = text;


        scrollBar.value = 0f;
        Fit(Bubble.BoxRect);
    }

    public void Choice(int index)
    {
        int answer = 0; // 임시
        if (answer != index) GameManager.Instance.ChangeEnding();
    }

    void Fit(RectTransform Rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);

    public void ExitChatroom()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
