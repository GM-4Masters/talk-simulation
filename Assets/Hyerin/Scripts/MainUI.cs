using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;

    private GameObject[] popup;

    private enum POPUP { CREDIT, RESET, EXIT }

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
        GameManager.Instance.GetAudioController().PlayEffect(AudioController.EFFECT.START);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ShowExitPopup()
    {
        ShowPopup(POPUP.EXIT);
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
        ClosePopup();
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
