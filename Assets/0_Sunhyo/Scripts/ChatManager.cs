using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Text;

public class ChatManager : MonoBehaviour
{
    public GameObject playerChatPrefab, npcChatPrefab;
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
    public Text firstTxt;
    public Text secondTxt;

    public GameObject DatePrefab;

    private string first, second;

    private Button chatbarBtn;

    private WaitForSeconds wait;

    private BubbleScript currentBubble;
    private GameObject currentDateBar;

    private PlayableDirector chatBarPd;

    private GameObject[] npcChat, playerChat, dateBar;
    private BubbleScript[] npcBubble, playerBubble;
    private Text[] dateBarTxt;
    private int totalChatCount = 50;
    private int totalDateBarCount = 10;
    private int activatedNpcChatCount = 0;
    private int activatedPlayerChatCount = 0;
    private int activatedDateBarCount = 0;

    private void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }

    private void Awake()
    {
        chatbarBtn = ChatBar.GetComponent<Button>();
        chatbarBtn.interactable = false;

        chatBarPd = ChatText.gameObject.GetComponent<PlayableDirector>();

        npcChat = new GameObject[totalChatCount];
        playerChat = new GameObject[totalChatCount];
        npcBubble = new BubbleScript[totalChatCount];
        playerBubble = new BubbleScript[totalChatCount];
        for (int i=0; i<totalChatCount; i++)
        {
            npcChat[i] = contentRect.GetChild(i).gameObject;
            npcBubble[i] = npcChat[i].GetComponent<BubbleScript>();

            playerChat[i] = contentRect.GetChild(totalChatCount+i).gameObject;
            playerBubble[i] = playerChat[i].GetComponent<BubbleScript>();
        }

        dateBar = new GameObject[totalDateBarCount];
        dateBarTxt = new Text[totalDateBarCount];
        for(int i=0; i<totalDateBarCount; i++)
        {
            dateBar[i] = contentRect.GetChild(i + totalChatCount * 2).gameObject;
            dateBarTxt[i] = dateBar[i].transform.GetChild(0).GetComponent<Text>();
        }
    }

    public void Chat(bool isSend, string text, string s_time, string name = "", string readCount = "", string img = "", string fileName = "")
    {
        if (isSend)
        {
            // �÷��̾� ä��
            if (activatedPlayerChatCount >= totalChatCount)
            {
                // �߰� ����
                currentBubble = Instantiate(playerChatPrefab).GetComponent<BubbleScript>();
                currentBubble.transform.SetParent(contentRect.transform, false);
            }
            else
            {
                // ���� ������Ʈ ���
                playerChat[activatedPlayerChatCount].SetActive(true);
                playerChat[activatedPlayerChatCount].transform.SetAsLastSibling();

                currentBubble = playerBubble[activatedPlayerChatCount];
                activatedPlayerChatCount++;
            }
        }
        else
        {
            // NPC ä��
            if (activatedNpcChatCount >= totalChatCount)
            {
                // �߰� ����
                currentBubble = Instantiate(npcChatPrefab).GetComponent<BubbleScript>();
                currentBubble.transform.SetParent(contentRect.transform, false);
            }
            else
            {
                // ���� ������Ʈ ���
                npcChat[activatedNpcChatCount].SetActive(true);
                npcChat[activatedNpcChatCount].transform.SetAsLastSibling();

                currentBubble = npcBubble[activatedNpcChatCount];
                activatedNpcChatCount++;
            }
        }




        // �����ʻ��� ����
        if(!isSend) currentBubble.ProfileImage.sprite = Resources.Load<Sprite>("Sprites/Profile/" + name);

        // ReadCount �ؽ�Ʈ ������Ʈ ����
        GameManager.Instance.AddReadCountTxt(currentBubble.ReadCountText);

        if (!isSend)
        {
            currentBubble.NameText.text = name;
        }

        currentBubble.ReadCountText.text = readCount;

        if (img != "")
        {
            currentBubble.BoxRect.sizeDelta = new Vector2(400, currentBubble.BoxRect.sizeDelta.y);
            currentBubble.TextRect.gameObject.SetActive(false);
            currentBubble.ChatImage.sprite = Resources.Load<Sprite>("Sprites/Image/" + img);
        }

        if(fileName != "")
        {
            currentBubble.BoxRect.sizeDelta = new Vector2(400, currentBubble.BoxRect.sizeDelta.y);
            currentBubble.TextRect.gameObject.SetActive(false);
            currentBubble.File.SetActive(true);
            currentBubble.FileNameTxt.text = fileName; 
        }

        // ���� ���� ���ٸ� ChatBox �ʺ� ����
        if (text.Length > 0 && Encoding.Default.GetByteCount(text) <= 45)
        {
            currentBubble.BoxRect.sizeDelta = new Vector2(98 + Encoding.Default.GetByteCount(text)*11, currentBubble.BoxRect.sizeDelta.y);
        }
        // ä�� �޽����� ������ '\u00A0'�� ����
        text = text.Replace(' ', '\u00A0');

        currentBubble.TimeText.text = s_time;
        currentBubble.ChatText.text = text;

        scrollBar.value = 0f;
        Fit(currentBubble.BoxRect);
    }

    public void SetDate(string date)
    {
        if (activatedDateBarCount >= totalDateBarCount)
        {
            // �߰� ����
            currentDateBar = Instantiate(DatePrefab);
            currentDateBar.transform.SetParent(contentRect.transform, false);
            currentDateBar.transform.GetChild(0).GetComponent<Text>().text = date;
        }
        else
        {
            // ���� ������Ʈ ���
            dateBar[activatedDateBarCount].transform.SetAsLastSibling();
            dateBar[activatedDateBarCount].SetActive(true);
            dateBarTxt[activatedDateBarCount].text = date;

            activatedDateBarCount++;
        }
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

        ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, 1);
        //ChatBar.SetActive(false);
        chatBarPd.Stop();
        chatbarBtn.interactable = false;

        ChatScreen.offsetMin = new Vector2(0, 550);   // stretch left, stretch bottom
        //ChatScreen.anchoredPosition = new Vector2(0, 145);
        //ChatScreen.sizeDelta = new Vector2(0, 880);
        bottomPanel.sizeDelta = new Vector2(0, 700);

        firstTxt.text = first;
        secondTxt.text = second;

        Choices.SetActive(true);

        scrollBar.value = 0f;
    }

    public void CloseChoice()
    {
        Choices.SetActive(false);

        ChatScreen.offsetMin = new Vector2(0, 0);
        //ChatScreen.anchoredPosition = new Vector2(0, -100);
        //ChatScreen.sizeDelta = new Vector2(0, 1370);
        bottomPanel.sizeDelta = new Vector2(0, 150);

        //ChatBar.SetActive(true);
        ChatText.text = "";
        answerState.text = "����";

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









    public void BackToChatList()
    {
        print("����Ʈ�� �̵�");
    }






    public void PrintAside(string text, float waitTime)
    {
        answerState.text = "����";
        StartCoroutine(PrintLikeKeyboard(text,waitTime));
    }

    IEnumerator PrintLikeKeyboard(string _text, float waitTime)
    {
        wait = new WaitForSeconds(waitTime);

        GameManager.Instance.Audio.PlayEffect(AudioController.EFFECT.TYPING);
        for (int i = 0; i < _text.Length + 1; i++)
        {
            yield return new WaitForSeconds(0.05f/GameManager.Instance.Speed);

            ChatText.text = _text.Substring(0, i);
        }

        GameManager.Instance.Audio.StopEffect();
        yield return wait;

        // delete text
        for (int i = _text.Length; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.02f / GameManager.Instance.Speed);

            ChatText.text = _text.Substring(0, i);
        }
    }


    public void BTNTEST()
    {
        SetAnswer();
    }


    public void SetAnswer(string _first = "", string _second = "")
    {
        first = _first;
        second = _second;

        ChatText.text = "ANSWER";
        answerState.text = "����";

        chatbarBtn.interactable = true;
        //StartCoroutine("FadeTextToZeroAlpha");
        chatBarPd.Play();
    }

    //public IEnumerator FadeTextToFullAlpha() // ���İ� 0���� 1�� ��ȯ
    //{
    //    ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, 0);
    //    while (ChatText.color.a < 1.0f)
    //    {
    //        ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, ChatText.color.a + (Time.deltaTime / 2.0f));
    //        yield return null;
    //    }
    //    StartCoroutine(FadeTextToZeroAlpha());
    //}

    //public IEnumerator FadeTextToZeroAlpha()  // ���İ� 1���� 0���� ��ȯ
    //{
    //    ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, 1);
    //    while (ChatText.color.a > 0.0f)
    //    {
    //        ChatText.color = new Color(ChatText.color.r, ChatText.color.g, ChatText.color.b, ChatText.color.a - (Time.deltaTime / 2.0f));
    //        yield return null;
    //    }
    //    StartCoroutine(FadeTextToFullAlpha());
    //}










}