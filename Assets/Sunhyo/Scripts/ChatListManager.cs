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
            if(instance == null)
            {
                Debug.Log("No ChatListManager Singleton");
                return null;
            }
            return instance;
        }
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
        isEntered[0] = true;
    }

    public void ResetState()
    {
        for(int i=0; i<isEntered.Length; i++)
        {
            isEntered[i] = false;
        }
        // 단톡방은 항상 읽음 상태임
        isEntered[0] = true;

        // 튜토리얼이 끝났다면 읽음처리
        if (GameManager.Instance.IsTutorialFinished)
        {
            isEntered[5] = true;
        }
    }

    public void Save()
    {
        // 각 채팅방의 읽음 상태 세이브
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