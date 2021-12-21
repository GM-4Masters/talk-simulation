using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject notice;
    [SerializeField] private GameObject popupMessage;   // �˾��޽���(���� �Ұ� ��)
    [SerializeField] private Image fadeoutImg;

    private PlayableDirector notificationPd;
    private PlayableDirector popupMessagePd;

    private bool isWait = false;        // ä�� ��� ����
    private float spendTime = 0f;

    private bool isFirstSelect = true;  // �� �������� ù��° ������
    private int episodeIndex;
    private int lastChatIndex;
    private int currentChatIndex;
    private int passedChoiceNum = -1;   // ����ģ ������ ����(-1���� ����)
    private int groupTalkUnchecked = 3; // ������ ��� ��(����� ����)
    private string chatroom;

    private int answerNum;

    ChatData currentChatData;

    private Text notiName, notiText;
    private Image notiImg;

    private List<ChatData> chatList;
    private List<Text> readCountTxt;
    private List<ChatData> personalChatList;
    private WaitForSeconds wait;

    private Color fadeoutColor;
    private Color black = new Color(0, 0, 0, 1);

    private string chatText,chatImagePath,chatFileName,readCountStr;
    private string answerStr, wrongStr;

    private void Awake()
    {
        notificationPd = notification.GetComponent<PlayableDirector>();
        notiName = notification.transform.GetChild(0).GetComponent<Text>();
        notiText = notification.transform.GetChild(1).GetComponent<Text>();
        notiImg = notification.transform.GetChild(2).GetComponent<Image>();

        popupMessagePd = popupMessage.GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        GameManager.Instance.ClearReadCount();

        fadeoutImg.color = new Color(0, 0, 0, 0);

        episodeIndex = GameManager.Instance.EpisodeIndex;
        chatroom = GameManager.Instance.Chatroom;
        chatList = DataManager.Instance.GetChatList(episodeIndex, chatroom);

        // ���� �� ���� ����
        int index = episodeIndex;
        string chatroomName = chatroom;
        if (chatroom.Equals(Constants.grouptalk)) chatroomName = Constants.grouptalkName;
        if (chatroom.Equals(Constants.mastertalk)) index = 0;
        chatManager.SetChatScreen(chatroomName, DataManager.Instance.noticeList[chatroom][index]);

        // �׷�ä�ù� �ƴϸ� ����ǥ�� ��Ȱ��ȭ
        if (!GameManager.Instance.Chatroom.Equals(Constants.grouptalk))
        {
            readCountTxt = GameManager.Instance.ReadCountTxt;
            for (int i = 0; i < readCountTxt.Count; i++)
            {
                readCountTxt[i].enabled = false;
            }
        }

        // ä�ù� ������ ���� ������ �ٷ� ����� ������ ���� 
        if (chatroom.Equals(Constants.grouptalk))
        {
            // �׷�ä�ù�
            lastChatIndex = GameManager.Instance.CurrentChatIndex;
        }
        else if (chatroom.Equals(Constants.mastertalk))
        {
            // Ʃ�丮��
            lastChatIndex = ((GameManager.Instance.IsTutorialFinished)? chatList.Count - 1 : 0);
        }
        else if (GameManager.Instance.IsEpisodeFinished &&
                chatroom.ToString().Equals(DataManager.Instance.GetPersonalChatName(episodeIndex)))
        {
            // ���Ǽҵ� �� ����ä�ù�
            chatList = DataManager.Instance.GetChatList(episodeIndex,DataManager.DATATYPE.PERSONAL);
            lastChatIndex = chatList.Count - 4;   //�̰Ÿ³�
        }
        else
        {
            // ����ä�ù�(�ѹ��� ���)
            lastChatIndex = chatList.Count - 1;
        }


        StartCoroutine(ChatCrt());
    }

    private IEnumerator ChatCrt()
    {
        currentChatIndex = lastChatIndex;

        // ��� ���� �ٷ� ����ؾ� �ϴ� �պκ� ä��
        for (int i=0; i<=currentChatIndex; i++)
        {
            currentChatData = chatList[i];

            if (chatroom.Equals(Constants.grouptalk))
            {
                if (DataManager.Instance.goodChoiceList[episodeIndex][passedChoiceNum + 1][0] == currentChatData.index ||
                    DataManager.Instance.badChoiceList[episodeIndex][passedChoiceNum + 1][0] == currentChatData.index)
                    passedChoiceNum++;
            }

            if (IsRightDialogue())
            {
                SetUI(i);
            }
        }
        GoNext();

        //Debug.Log(currentChatIndex+", last:"+(chatList.Count-1));
        // ��� ä�� ����� ���������� ����
        while (currentChatIndex < chatList.Count)
        {
            if (!isWait)
            {
                if (IsRightDialogue())
                {
                    // ��� ��
                    if (spendTime < currentChatData.dt)
                    {
                        spendTime += (Time.deltaTime * GameManager.Instance.Speed);
                    }
                    else
                    {
                        // ���

                        switch (currentChatData.character)
                        {
                            case Constants.monologue:
                                // ���� �ִϸ��̼� ���� �� �������� �Ѿ
                                float waitTime = float.Parse(currentChatData.text.Substring(currentChatData.text.Length - 3, 1));
                                chatManager.PrintAside(currentChatData.text.Substring(0, currentChatData.text.Length - 4), waitTime);
                                yield return new WaitForSeconds(waitTime * 3);
                                isWait = false;
                                break;
                            case Constants.choice:
                                WaitForSelect();
                                break;
                            case Constants.gameStart:
                                isWait = false;
                                break;
                            default:
                                isWait = false;
                                break;
                        }
                        // ��ȭ�� ä�ÿ����� ȿ���� ���
                        if (Constants.talkable.Contains(currentChatData.character))
                            GameManager.Instance.Audio.PlayEffect(AudioController.EFFECT.POP);

                        SetUI(currentChatIndex);
                        GoNext();
                    }
                }
                else
                {
                    GoNext();
                }
            }

            yield return null;
        }

        // �׷�ä�ù濡�� ��� ä�� ����� ������ ��
        if (chatroom.Equals(Constants.grouptalk) && !GameManager.Instance.IsEpisodeFinished)
        {
            wait = new WaitForSeconds(2f);

            if (episodeIndex!=2 && GameManager.Instance.Ending == GameManager.ENDING.NORMAL)
            {
                // ������ ���Ǽҵ尡 �ƴϰ� ��忣���� �ƴ϶�� ���θ޽��� �˾� ǥ��
                personalChatList = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
                for (int i=1; i < 3; i++)
                {
                    yield return wait;
                    notificationPd.Stop();
                    //// �÷��̾� �޽��� ���������� �ߵ���
                    //if (personalChatList[i].character.Equals(Constants.me)) break;

                    notiName.text = personalChatList[i].character;
                    notiText.text = personalChatList[i].text;
                    notiImg.sprite = Resources.Load<Sprite>("Sprites/Profile/" + personalChatList[i].character);

                    // �ڷ�ƾ ��� �ִϸ����͸� Ű��
                    notificationPd.Play();
                    GameManager.Instance.Audio.PlayEffect(AudioController.EFFECT.ALERT);
                }

                // ������ �޽��� ���̺�
                GameManager.Instance.CurrentChatIndex = DataManager.Instance.GetLastMainChat(episodeIndex);
                GameManager.Instance.IsEpisodeFinished = true;
                GameManager.Instance.Save();

                // 2�� ��� �� ä�ù� �ڵ����� ������
                yield return wait;
                //GameManager.Instance.ClearReadCount();
                GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
            }
            else
            {
                // �׷��� ������ ���̵�ƿ� �� ����
                yield return wait;

                // ���̵�ƿ�
                float time = 0f;
                fadeoutColor = fadeoutImg.color;
                while (time < 2f)
                {
                    time += (Time.deltaTime * GameManager.Instance.Speed);
                    fadeoutColor.a = time * 0.5f;
                    fadeoutImg.color = fadeoutColor;
                    yield return null;
                }
                fadeoutImg.color = black;

                //GameManager.Instance.ClearReadCount();
                GameManager.Instance.ChangeScene(GameManager.SCENE.ENDING);
            }
        }
    }

    private void GoNext()
    {
        currentChatIndex++;
        if(currentChatIndex<chatList.Count) currentChatData = chatList[currentChatIndex];
        spendTime = 0f;
    }

    // ��� ��� ��ȭ���� �Ǵ�(�������� ����)
    private bool IsRightDialogue()
    {
        if (passedChoiceNum < 0) return true;


        // ������ �б����̶�� ������, �ƴϸ� ������ ���� ���η� ������ �����ϰ�
        // ���� ä���� �ش� ������ ���ϴ����� �Ǵ�
        if (DataManager.Instance.IsLastDialogueSet(episodeIndex, currentChatData.index))
        {
            bool isGoodEnding = (GameManager.Instance.Ending == GameManager.ENDING.NORMAL);
            return (DataManager.Instance.IsInRange(currentChatData.index, isGoodEnding, episodeIndex, passedChoiceNum+1));
        }
        else
        {
            bool myChoice = GameManager.Instance.IsChoiceRight[passedChoiceNum];
            return (DataManager.Instance.IsInRange(currentChatData.index, myChoice, episodeIndex, passedChoiceNum));
        }
    }

    public void SetUI(int index)
    {
        if (Constants.talkable.Contains(currentChatData.character))
        {
            bool isSend = (currentChatData.character.Equals(Constants.me));
            chatText = "";
            chatImagePath = "";
            chatFileName = "";
            if (currentChatData.text.Contains("image")) chatImagePath = currentChatData.text;
            else if (currentChatData.text.Contains("+")) chatFileName = currentChatData.text.Substring(1);
            else chatText = currentChatData.text;

            // ������̰� ���������ڰ� 0�� �ƴϸ� �������� ǥ��
            readCountStr = "";
            if (GameManager.Instance.Chatroom.Equals(Constants.grouptalk) &&
                groupTalkUnchecked != 0)
            {
                readCountStr = groupTalkUnchecked.ToString();
            }
            chatManager.Chat(isSend, chatText, currentChatData.time, currentChatData.character, readCountStr, chatImagePath, chatFileName);
        }

        switch (currentChatData.character)
        {
            case "�ý���":
                chatManager.SetDate(currentChatData.text);
                break;
            case "����":
                ChangeReaderCount(-currentChatData.enterNum);
                break;
            case "����":
                ChangeReaderCount(currentChatData.enterNum);
                break;
            default:
                break;
        }
    }

    public void WaitForSelect()
    {
        if (isFirstSelect)
        {
            isFirstSelect = false;
            isWait = false;
            return;
        }
        // ������ ���� �� ���(���� ��ġ ����)
        isWait=true;
        int answerNum = Random.Range(0, 2);
        this.answerNum = answerNum;

        answerStr = chatList[currentChatIndex-1].text;
        wrongStr = currentChatData.text;
        if (answerNum == 0) chatManager.SetAnswer(answerStr, wrongStr);
        else chatManager.SetAnswer(wrongStr, answerStr);
        isFirstSelect = true;
    }

    // ������ ����
    public void Select(int selected)
    {
        chatManager.CloseChoice();
        //Debug.Log("selected:"+selected+", answer:"+answerNum);
        GameManager.Instance.AddChoiceResult(answerNum == selected);
        if (answerNum != selected) GameManager.Instance.ChangeEnding();
        passedChoiceNum++;
        GameManager.Instance.ChoiceNum++;

        if (answerNum == selected)
        {
            GameManager.Instance.CurrentChatIndex = currentChatIndex;
        }
        else
        {
            GameManager.Instance.CurrentChatIndex = currentChatIndex + DataManager.Instance.GetBadEndingOffset(episodeIndex, passedChoiceNum);
        }
        GameManager.Instance.Save();
        spendTime += 1000f;
        isWait=false;
    }

    public void ChangeReaderCount(int value)
    {
        groupTalkUnchecked += value;
        readCountTxt = GameManager.Instance.ReadCountTxt;
        for(int i=0; i<readCountTxt.Count; i++)
        {
            readCountStr = (groupTalkUnchecked == 0) ? 
                "" : groupTalkUnchecked.ToString();
            if (readCountTxt[i].text!="" && int.Parse(readCountTxt[i].text)> groupTalkUnchecked)
                readCountTxt[i].text = readCountStr;
        }
    }



    public void ExitChatroom()
    {
        // �׷�ä�ù� - ���� �� ���� (�˾� ����)
        if (GameManager.Instance.Chatroom.Equals(Constants.grouptalk) && !GameManager.Instance.IsEpisodeFinished)
        {
            popupMessagePd.Play();
            return;
        }

        // ����ä�ù� - ���� �� ���� ���Ǽҵ�� �̵�
        if (GameManager.Instance.IsEpisodeFinished &&
            GameManager.Instance.Chatroom.ToString().Equals(DataManager.Instance.GetPersonalChatName(episodeIndex)))
            GameManager.Instance.GoToNextEpisode();

        // Ʃ�丮��ä�ù�(���� Ʃ�丮�� �Ϸ�) - ù��° ���Ǽҵ�� �̵�
        if (GameManager.Instance.Chatroom.Equals(Constants.mastertalk) &&
            !GameManager.Instance.IsTutorialFinished)
            GameManager.Instance.FinishTutorial();

        // �Ϲ� ������(����ä�ù�, ���Ǽҵ� ����� �׷�ä�ù�, Ʃ�丮�� �Ϸ�� Ʃ�丮��ä�ù�)
        //GameManager.Instance.ClearReadCount();
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }
}
