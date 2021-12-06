using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Linq;
using System.IO;
using System;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    private List<ChatData> chatDataList;

    public static DataManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(DataManager)) as DataManager;

                if (instance == null)
                    Debug.Log("no Singleton obj");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        LoadData();
    }

    private void LoadData()
    {
        //textDataList = JsonConvert.DeserializeObject<List<TextData>>(Resources.Load<TextAsset>($"Data/TextData").text);
    }

    public ChatData GetChatData(int episodeIndex, int chatIndex)
    {
        return chatDataList[chatIndex];
    }
}

public class ChatData
{
    private int index;
    private float dt;
    private int date;
    private int time;
    private int characterIndex; // 0:시스템, 1~4:사람
    private string text;        // 대화내용

    public float GetDt()
    {
        return dt;
    }

    public int GetDate()
    {
        return date;
    }

    public int GetTime()
    {
        return time;
    }

    public int getCharacterIndex()
    {
        return characterIndex;
    }

    public string GetText()
    {
        return text;
    }

    public bool IsSystem()
    {
        return (characterIndex == 0);
    }
}