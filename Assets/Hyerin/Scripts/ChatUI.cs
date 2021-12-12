using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject notice;
    [SerializeField] private GameObject popupMessage;   // �˾��޽���(���� �Ұ� ��)
    [SerializeField] private Image fadeoutImg;

    ChatData chatData;

    private bool isGameStart = false;
    private bool isFirstSelect = true;
    private int chatroomIndex;
    private int episodeIndex;

    private int answerNum;

    private Image notificationBg;
    private Text notiName, notiText;
    private Image popupMessageImg;
    private Text popupMessageTxt;

    private List<ChatData> data;

    string talkable = "�����,���ȣ,������,��ä��,����,GameMasters,4MasterTalk";

    private IEnumerator notificationCrt, popupEffectCrt;

    private void Awake()
    {
        notificationBg = notification.transform.GetComponent<Image>();
        notiName = notification.transform.GetChild(0).GetComponent<Text>();
        notiText = notification.transform.GetChild(1).GetComponent<Text>();

        popupMessageImg = popupMessage.GetComponent<Image>();
        popupMessageTxt = popupMessage.transform.GetChild(0).GetComponent<Text>();
    }

    private void OnEnable()
    {
        fadeoutImg.color = new Color(0, 0, 0, 0);

        episodeIndex = GameManager.Instance.GetEpisodeIndex();
        chatroomIndex = DataManager.Instance.chatroomList.IndexOf(GameManager.Instance.chatroom);
        data = DataManager.Instance.GetChatList(episodeIndex, GameManager.Instance.chatroom);

        if (episodeIndex >= 0)
        {
            // ���� �� ���� ����
            string chatroomName = GameManager.Instance.chatroom;
            if (chatroomIndex == 0) chatroomName = "4Master��";
            else if (GameManager.Instance.chatroom == "Ʃ�丮��") chatroomName = "4MasterTalk";
            chatManager.SetChatScreen(chatroomName, DataManager.Instance.noticeList[chatroomIndex][episodeIndex]);


            // ���� ä�ù��� ���Ǽҵ� ���� �����̸� ���Ǽҵ� �� ���� ����
            if (GameManager.Instance.IsEpisodeFinished() && GameManager.Instance.chatroom.Equals(DataManager.Instance.GetPersonalChatName(episodeIndex)))
            {
                StartCoroutine(PersonalTalkCrt());
            }
            else if (chatroomIndex != 0)
            {
                // ������ �̿��� ��� ����
                for (int i = 0; i < data.Count; i++)
                {
                    chatData = data[i];
                    SetUI();
                }

            }
        }
        else
        {
            // ���� �� ���� ����
            string chatroomName = "4MasterTalk";
            chatManager.SetChatScreen(chatroomName, DataManager.Instance.noticeList[chatroomIndex][0]);

            // Ʃ�丮�� �������������� �ϳ��� ���
            StartCoroutine(TutorialTalkCrt());
        }



        if (chatroomIndex != 0)
        {
            // ������ �ƴϸ� ����ǥ�� ��Ȱ��ȭ
            List<Text> readCountTxt = GameManager.Instance.GetReadCountTxt();
            for (int i = 0; i < readCountTxt.Count; i++)
            {
                readCountTxt[i].enabled = false;
            }
        }
    }

    private void Update()
    {
        // �� ������ ���
        if (chatroomIndex == 0)
        {
            int recentIndex = GameManager.Instance.currentChatIndex;

            // ���� ����� ���� ����ٸ�
            if (GameManager.Instance.lastChatIndex != recentIndex)
            {
                for(int i=GameManager.Instance.lastChatIndex+1; i<=recentIndex; i++)
                {
                    chatData = DataManager.Instance.GetChatData(episodeIndex, i);
                    // �� ���忡�� �������� �޾��� ��
                    if (chatData.chatroom != "������")
                    {
                        if (GameManager.Instance.GetEndingType()==GameManager.ENDING.NORMAL)
                        {
                            ShowNotification();
                        }
                        if ( recentIndex == GameManager.Instance.mainEpisodeCnt[episodeIndex] - 1)
                        {
                            if (GameManager.Instance.GetEndingType() == GameManager.ENDING.NORMAL)
                            {
                                StartCoroutine(WaitAndExit(GameManager.SCENE.CHATLIST, 2f));
                            }
                            else
                            {
                                StartCoroutine(WaitAndExit(GameManager.SCENE.ENDING, 2f));
                            }
                        }
                    }
                    else
                    {
                        // ������ ���Ǽҵ� �� ����
                        if (recentIndex == GameManager.Instance.mainEpisodeCnt[episodeIndex] - 1)
                        {
                            float waitTime = float.Parse(chatData.text.Substring(chatData.text.Length - 3, 1));
                            StartCoroutine(WaitAndExit(GameManager.SCENE.ENDING, waitTime));
                        }
                        Play();
                        SetUI();
                    }
                    GameManager.Instance.lastChatIndex = recentIndex;
                }
            }
        }
    }

    // ��� ��� ��ȭ���� �Ǵ�(�������� ����)
    private bool IsRightDialogue()
    {
        int choiceNum = GameManager.Instance.choiceNum;

        if (choiceNum < 0) return true;

        // ������ �б����̶�� ������, �ƴϸ� ������ ���� ���η� ������ �����ϰ�
        // ���� ä���� �ش� ������ ���ϴ����� �Ǵ�
        if (DataManager.Instance.IsLastDialogueSet(episodeIndex, chatData.index))
        {
            bool isGoodEnding = (GameManager.Instance.ending == GameManager.ENDING.NORMAL);
            return (DataManager.Instance.IsInRange(chatData.index, isGoodEnding, episodeIndex));
        }
        else return (DataManager.Instance.IsInRange(chatData.index, GameManager.Instance.isChoiceRight, episodeIndex));
    }

    public void Play()
    {
        if (!IsRightDialogue())
        {
            //Debug.Log("play skipped:" + chatData.index);
            GameManager.Instance.SkipTime();
            GameManager.Instance.ChangeWaitFlag(false);
            return;
        };

        switch (chatData.character)
        {
            case "����":
                Monologue();
                break;
            case "������":
                WaitForSelect();
                break;
            case "���ӽ���":
                isGameStart = true;
                GameManager.Instance.ChangeWaitFlag(false);
                break;
            default:
                GameManager.Instance.ChangeWaitFlag(false);
                break;
        }

        if (isGameStart && (talkable.Contains(chatData.character) || chatData.character == "��Ʈ��"))
        {
            GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.POP);
        }
    }

    public void SetUI()
    {
        if (!IsRightDialogue())
        {
            //Debug.Log("setui skipped:" + chatData.index);
            return;
        };
        if (talkable.Contains(chatData.character) || chatData.character=="��Ʈ��")
        {
            bool isSend = (chatData.character=="��Ʈ��");
            string text = "";
            string image = "";
            string fileName = "";
            if (chatData.text.Contains("image")) image = chatData.text;
            else if (chatData.text.Contains("+")) fileName = chatData.text.Substring(1);
            else text = chatData.text;

            // ������̰� ���������ڰ� 0�� �ƴϸ� �������� ǥ��
            string numStr = "";
            if (GameManager.Instance.chatroom == DataManager.Instance.chatroomList[0] &&
                GameManager.Instance.groupTalkUnChecked != 0)
            {
                numStr = GameManager.Instance.groupTalkUnChecked.ToString();
            }
            chatManager.Chat(isSend, text, chatData.time, chatData.character, numStr, image, fileName);
        }

        switch (chatData.character)
        {
            case "�ý���":
                chatManager.SetDate(chatData.text);
                break;
            case "����":
                ChangeReaderCount(-chatData.enterNum);
                break;
            case "����":
                ChangeReaderCount(chatData.enterNum);
                break;
            default:
                break;
        }
    }

    public void Monologue()
    {
        // ���� �ִϸ��̼� ���� �� �������� �Ѿ
        float waitTime = float.Parse(chatData.text.Substring(chatData.text.Length - 3, 1));
        chatManager.PrintAside(chatData.text.Substring(0, chatData.text.Length - 4), waitTime);
        StartCoroutine(MonologueCrt(waitTime * 3));
    }

    private IEnumerator MonologueCrt(float second)
    {
        yield return new WaitForSeconds(second);
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void WaitForSelect()
    {
        if (isFirstSelect)
        {
            isFirstSelect = false;
            GameManager.Instance.GoNext();
            return;
        }
        // ������ ���� �� ���(���� ��ġ ����)
        GameManager.Instance.ChangeWaitFlag(true);
        int answerNum = Random.Range(0, 2);
        this.answerNum = answerNum;

        string answer = DataManager.Instance.GetChatData(episodeIndex,
            GameManager.Instance.currentChatIndex - 1).text;
        string wrong = chatData.text;
        if (answerNum == 0) chatManager.SetAnswer(answer, wrong);
        else chatManager.SetAnswer(wrong, answer);
        isFirstSelect = true;
    }

    public void Select(int selected)
    {
        //GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.)
        chatManager.CloseChoice();
        //Debug.Log("selected:"+selected+", answer:"+answerNum);
        GameManager.Instance.isChoiceRight = (answerNum == selected);
        if (answerNum != selected) GameManager.Instance.ChangeEnding();
        GameManager.Instance.choiceNum++;
        GameManager.Instance.ChangeWaitFlag(false);
    }

    public void ChangeReaderCount(int value)
    {
        GameManager.Instance.groupTalkUnChecked += value;
        List<Text> readCountTxt = GameManager.Instance.GetReadCountTxt();
        for(int i=0; i<readCountTxt.Count; i++)
        {
            string numStr = (GameManager.Instance.groupTalkUnChecked == 0) ? 
                "" : GameManager.Instance.groupTalkUnChecked.ToString();
            if (readCountTxt[i].text!="" && int.Parse(readCountTxt[i].text)> GameManager.Instance.groupTalkUnChecked)
                readCountTxt[i].text = numStr;
        }
    }

    public void ExitChatroom()
    {
        //isDisplayed = false;
        if (chatroomIndex == 0)
        {
            // ���Ǽҵ� ���� �߿��� ���� �� ����
            if (popupEffectCrt != null) StopCoroutine(popupEffectCrt);
            popupEffectCrt = PopupEffectCrt();
            StartCoroutine(popupEffectCrt);
            return;
        }

        // ���� ���� �� ���� ���Ǽҵ�
        if (GameManager.Instance.IsEpisodeFinished() &&
            GameManager.Instance.chatroom.Equals(DataManager.Instance.GetPersonalChatName(episodeIndex))) GameManager.Instance.GoToNextEpisode();

        // Ʃ�丮�� �Ϸ�(���� ���Ǽҵ�)
        if (chatroomIndex == 5 && !GameManager.Instance.IsTutorialFinished()) GameManager.Instance.FinishTutorial();

        // �Ϲ� ä�ù� ������
        //GameManager.Instance.Save();
        GameManager.Instance.ClearReadCount();
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }

    private IEnumerator PopupEffectCrt()
    {
        popupMessage.SetActive(true);

        float time = 0f;
        Color bgColor = popupMessageImg.color;  // 0f ~ 0.5f
        Color txtColor = popupMessageTxt.color; // 0f ~ 1f
        while (time < 2f)
        {
            time += Time.deltaTime;
            bgColor.a = Mathf.Clamp((Mathf.Sin(time * 2) * 2), 0, 1) * 0.5f;
            txtColor.a = Mathf.Clamp((Mathf.Sin(time * 2) * 2), 0, 1);
            popupMessageImg.color = bgColor;
            popupMessageTxt.color = txtColor;
            yield return null;
        }

        popupMessage.SetActive(false);
    }


    private IEnumerator TutorialTalkCrt()
    {
        List<ChatData> tutorialChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.TUTORIAL);
        for (int i = 0; i < tutorialChat.Count; i++)
        {
            GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.POP);
            chatData = tutorialChat[i];
            SetUI();
            yield return new WaitForSeconds(2f);
        }
        yield return null;
    }

    private IEnumerator PersonalTalkCrt()
    {
        List<ChatData> personalChat = DataManager.Instance.GetChatList(episodeIndex, DataManager.DATATYPE.PERSONAL);
        for(int i=0; i<personalChat.Count; i++)
        {
            bool isSend = (personalChat[i].character == "��Ʈ��");
            if (i > 2)
            {
                yield return new WaitForSeconds(chatData.dt);
                GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.POP);
            }
            chatData = personalChat[i];
            SetUI();

            //if(i<3) chatManager.Chat(
            //    isSend,
            //    personalChat[i].text, chatData.time, chatData.character, GameManager.Instance.groupTalkUnChecked.ToString(), image, fileName);
        }
        yield return null;
    }

    private IEnumerator WaitAndExit(GameManager.SCENE scene, float waitTime)
    {
        Debug.Log("waitTime:" + waitTime);
        yield return new WaitForSeconds(waitTime);

        // �������� ���� ���̸� ���̵�ƿ�
        if (scene == GameManager.SCENE.ENDING)
        {
            float time = 0f;
            Color color = fadeoutImg.color;
            while (time < 2f)
            {
                time += (Time.deltaTime * GameManager.Instance.GetPlaySpeed());
                color.a = time * (1 / 2f);
                fadeoutImg.color = color;
                yield return null;
            }
        }
        fadeoutImg.color = new Color(0, 0, 0, 1);

        GameManager.Instance.ClearReadCount();
        GameManager.Instance.ChangeScene(scene);
    }

    // �˸� 
    public void ShowNotification()
    {
        notiName.text = chatData.character;
        notiText.text = chatData.text;

        if (notificationCrt != null) StopCoroutine(notificationCrt);
        notificationCrt = NotificationCrt();
        StartCoroutine(notificationCrt);
    }

    private IEnumerator NotificationCrt()
    {
        GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.ALERT);

        // ���� 0(0.5��) -> 0.7(1�ʴ��) -> (0.5��)0 ���� �˸�â ���
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            notificationBg.color = new Color(1f, 1f, 1f, time*1.4f);
            notiName.color = new Color(0f, 0f, 0f, time * 2f);
            notiText.color = new Color(0f, 0f, 0f, time * 2f);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        while (time > 0f)
        {
            time -= Time.deltaTime;
            notificationBg.color = new Color(1f, 1f, 1f, time*1.4f);
            notiName.color = new Color(0f, 0f, 0f, time * 2f);
            notiText.color = new Color(0f, 0f, 0f, time * 2f);
            yield return null;
        }
        notificationBg.color = new Color(1f, 1f, 1f, 0f);
        notiName.color = new Color(0f, 0f, 0f, 0f);
        notiText.color = new Color(0f, 0f, 0f, 0f);
    }
}
