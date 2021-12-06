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

    public void Chat(bool isSend, string text, string name)
    {
        BubbleScript Bubble = Instantiate(isSend ? playerBubble : npcBubble).GetComponent<BubbleScript>();
        Bubble.transform.SetParent(contentRect.transform, false);
        Bubble.TextRect.GetComponent<Text>().text = text;


        scrollBar.value = 0f;
        Fit(Bubble.BoxRect);
    }

    void Fit(RectTransform Rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
}