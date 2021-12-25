using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menuPopup, exitPopup;
    [SerializeField] private Slider bgmSlider, effectSlider;
    [SerializeField] private AudioController audioController;

    private GameObject menuPanel;
    private bool isMenuPopupActive = false;

    private void Awake()
    {
        menuPanel = transform.GetChild(0).gameObject;

        audioController.LoadVolume();
        bgmSlider.value = audioController.GetBgmVolume() * 5f;
        effectSlider.value = audioController.GetEffectVolume() * 5f;

        bgmSlider.onValueChanged.AddListener(ChangeBgmVolume);
        effectSlider.onValueChanged.AddListener(ChangeEffectVolume);
    }

    private void Update()
    {
        #if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isMenuPopupActive)
            {
                menuPanel.SetActive(true);
                ShowMenuPopup();
            }
            else Close();
        }
        #endif
    }

    private void ChangeBgmVolume(float value)
    {
        audioController.SetBgmVolume(value * 0.2f);
    }

    private void ChangeEffectVolume(float value)
    {
        audioController.PlayEffect(AudioController.EFFECT.POP);
        audioController.SetEffectVolume(value*0.2f);
    }

    public void ShowMenuPopup()
    {
        Time.timeScale = 0f;
        audioController.Pause();
        isMenuPopupActive = true;
        menuPopup.SetActive(true);
        exitPopup.SetActive(false);
    }

    #region <Button> 컴포넌트에서 수동으로 연결한 함수

    public void ShowExitPopup()
    {
        isMenuPopupActive = false;
        menuPopup.SetActive(false);
        exitPopup.SetActive(true);
    }

    public void GoMain()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.MAIN);
        Close();
    }

    public void Close()
    {
        Time.timeScale = 1f;
        audioController.Resume();
        isMenuPopupActive = false;
        menuPanel.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
    #endregion Button 컴포넌트에서 사용
}
