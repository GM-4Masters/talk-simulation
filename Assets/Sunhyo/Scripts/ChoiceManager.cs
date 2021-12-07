using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    public RectTransform midPanel;
    public RectTransform bottomPanel;
    public Scrollbar scrollBar;
    public GameObject ChatBar;
    public GameObject Choices;

    public void ShowChoice()
    {
        ChatBar.SetActive(false);

        midPanel.anchoredPosition = new Vector2(0, 220);
        midPanel.sizeDelta = new Vector2(0, 1040);
        bottomPanel.sizeDelta = new Vector2(0, 640);
        
        Choices.SetActive(true);

        scrollBar.value = 0f;
    }

    public void CloseChoice()
    {
        Choices.SetActive(false);

        midPanel.anchoredPosition = new Vector2(0, -25);
        midPanel.sizeDelta = new Vector2(0, 1570);
        bottomPanel.sizeDelta = new Vector2(0, 150);

        ChatBar.SetActive(true);

        scrollBar.value = 0f;
    }
}
