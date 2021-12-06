using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Button loginBtn;
    [SerializeField] private GameObject popupPanel;

    private GameObject[] popup; // credit, reset

    private enum POPUP { CREDIT, RESET }

    private void Awake()
    {
        popup = new GameObject[popupPanel.transform.childCount];
        for (int i = 0; i < popup.Length; i++)
        {
            popup[i] = popupPanel.transform.GetChild(i).gameObject;
            popup[i].SetActive(false);
        }
    }

    public void Login()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.CHATLIST);
    }

    public void ShowCredit()
    {
        ShowPopup(POPUP.CREDIT);
    }

    public void ShowResetPopup()
    {
        ShowPopup(POPUP.RESET);
    }

    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
    }

    private void ShowPopup(POPUP popupType)
    {
        popup[(int)popupType].SetActive(true);
        popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        for (int i = 0; i < popup.Length; i++)
        {
            popup[i].SetActive(false);
        }
        popupPanel.SetActive(false);
    }
}
