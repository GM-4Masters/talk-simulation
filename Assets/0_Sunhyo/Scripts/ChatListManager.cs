using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatListManager : MonoBehaviour
{
    private bool[] isEntered = new bool[20];

    private static ChatListManager instance;
    public static ChatListManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(ChatListManager)) as ChatListManager;

                if (instance == null)
                    Debug.Log("no Singleton obj");
            }
            return instance;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ResetState()
    {
        for(int i=0; i<isEntered.Length; i++)
        {
            isEntered[i] = false;
        }

        // Ʃ�丮���� �����ٸ� ����ó��
        if (GameManager.Instance.IsTutorialFinished)
        {
            isEntered[1] = true;
        }
    }

    public void Save()
    {
        // �� ä�ù��� ���� ���� ���̺�
        for (int i = 0; i < DataManager.Instance.chatroomList.Count; i++)
        {
            int boolValue = ChatListManager.Instance.GetIsEntered(i) ? 1 : 0;
            PlayerPrefs.SetInt("IsEntered_" + i, boolValue);
        }
    }

    public void SetIsEntered(int idx, bool _isEntered)
    {
        isEntered[idx] = _isEntered;
    }

    public bool GetIsEntered(int idx)
    {
        return isEntered[idx];
    }
}