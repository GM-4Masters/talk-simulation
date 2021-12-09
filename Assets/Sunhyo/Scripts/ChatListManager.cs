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
    }

    public void ResetState()
    {
        for(int i=0; i<isEntered.Length; i++)
        {
            isEntered[i] = false;
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