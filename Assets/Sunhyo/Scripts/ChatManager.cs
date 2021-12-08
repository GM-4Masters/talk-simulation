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
    public Text roomName;
    public Text topic;
    public RectTransform ChatScreen;
    public RectTransform bottomPanel;
    public GameObject ChatBar;
    public GameObject Choices;
    public Text ChatText;
    public Text answerState;

    private string first, second, third;

    public void Chat(bool isSend, string text, string s_time, string name = "", string readCount = "", string img = "", string fileName = "")
    {
        BubbleScript Bubble = Instantiate(isSend ? playerBubble : npcBubble).GetComponent<BubbleScript>();
        Bubble.transform.SetParent(contentRect.transform, false);

        if(!isSend)
        {
            Bubble.NameText.text = name;
            Bubble.ReadCountText.text = readCount;
        }

        if(img != "")
        {
            Bubble.TextRect.gameObject.SetActive(false);
            Bubble.ChatImage.sprite = Resources.Load<Sprite>("Sprites/Image/" + img);
        }

        if(fileName != "")
        {
            Bubble.TextRect.gameObject.SetActive(false);
            Bubble.File.SetActive(true);
            Bubble.FileNameTxt.text = fileName; 
        }

        Bubble.TimeText.text = s_time;
        Bubble.TextRect.GetComponent<Text>().text = text;

        scrollBar.value = 0f;
        Fit(Bubble.BoxRect);
    }

    void Fit(RectTransform Rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);








    public void SetChatScreen(string _roomName, string _topic)
    {
        roomName.text = _roomName;
        topic.text = _topic;
    }












    public void ShowChoice()
    {
        StopAllCoroutines();

        ChatBar.SetActive(false);

        ChatScreen.anchoredPosition = new Vector2(0, 145);
        ChatScreen.sizeDelta = new Vector2(0, 880);
        bottomPanel.sizeDelta = new Vector2(0, 640);

        Text firstTxt = Choices.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
        Text secondTxt = Choices.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>();
        Text thirdTxt = Choices.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>();

        firstTxt.text = first;
        secondTxt.text = second;
        thirdTxt.text = third;

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
        print("ù��°");
    }

    public void SecondChoice()
    {
        print("�ι�°");
    }

    public void ThirdChoice()
    {
        print("����°");
    }









    public void BackToChatList()
    {
        print("����Ʈ�� �̵�");
    }









    public void PrintAside(string text)
    {
        answerState.text = "����";
        StartCoroutine(PrintLikeKeyboard(text));
    }

    IEnumerator PrintLikeKeyboard(string _text)
    {
        for (int i = 0; i < _text.Length + 1; i++)
        {
            yield return new WaitForSeconds(0.1f);

            ChatText.text = _text.Substring(0, i);
        }

        StartCoroutine(DeleteString(_text));
    }

    IEnumerator DeleteString(string _text)
    {
        for (int i = _text.Length; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.05f);

            ChatText.text = _text.Substring(0, i);
        }
    }







    public void BTNTEST()
    {
        SetAnswer();
    }


    public void SetAnswer(string _first = "", string _second = "", string _third = "")
    {
        first = _first;
        second = _second;
        third = _third;

        ChatText.text = "ANSWER";
        answerState.text = "����";

        StartCoroutine("FadeTextToZeroAlpha");
    }

    public IEnumerator FadeTextToFullAlpha() // ���İ� 0���� 1�� ��ȯ
    {
        ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, 0);
        while (ChatText.color.a < 1.0f)
        {
            ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, ChatText.color.a + (Time.deltaTime / 2.0f));
            yield return null;
        }
        StartCoroutine(FadeTextToZeroAlpha());
    }

    public IEnumerator FadeTextToZeroAlpha()  // ���İ� 1���� 0���� ��ȯ
    {
        ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, 1);
        while (ChatText.color.a > 0.0f)
        {
            ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, ChatText.color.a - (Time.deltaTime / 2.0f));
            yield return null;
        }
        StartCoroutine(FadeTextToFullAlpha());
    }










}