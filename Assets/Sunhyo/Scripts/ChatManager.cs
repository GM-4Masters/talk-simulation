using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public GameObject playerBubble, npcBubble;
    public RectTransform contentRect;
    public Scrollbar scrollBar;

    public RectTransform ChatScreen;
    public RectTransform bottomPanel;
    public GameObject ChatBar;
    public GameObject Choices;

    public void Chat(bool isSend, string text, string name)
    {
        BubbleScript Bubble = Instantiate(isSend ? playerBubble : npcBubble).GetComponent<BubbleScript>();
        Bubble.transform.SetParent(contentRect.transform, false);
        Bubble.TextRect.GetComponent<Text>().text = text;

        scrollBar.value = 0f;
        Fit(Bubble.BoxRect);
    }

    void Fit(RectTransform Rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);

    public void ShowChoice()
    {
        ChatBar.SetActive(false);

        ChatScreen.anchoredPosition = new Vector2(0, 145);
        ChatScreen.sizeDelta = new Vector2(0, 880);
        bottomPanel.sizeDelta = new Vector2(0, 640);

        Choices.SetActive(true);

        scrollBar.value = 0f;
    }

    public void CloseChoice()
    {
        Choices.SetActive(false);

        ChatScreen.anchoredPosition = new Vector2(0, -100);
        ChatScreen.sizeDelta = new Vector2(0, 1370);
        bottomPanel.sizeDelta = new Vector2(0, 150);

        ChatBar.SetActive(true);

        scrollBar.value = 0f;
    }

    public void FisrtChoice()
    {
        print("첫번째");
    }

    public void SecondChoice()
    {
        print("두번째");
    }

    public void ThirdChoice()
    {
        print("세번째");
    }

    public void BackToChatList()
    {
        print("리스트로 이동");
    }
}