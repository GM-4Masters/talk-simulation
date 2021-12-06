using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    private Text endingTxt;
    private Button goMainBtn;

    private void Awake()
    {
        endingTxt = gameObject.transform.GetChild(0).GetComponent<Text>();
        goMainBtn = gameObject.transform.GetChild(1).GetComponent<Button>();
    }

    private void OnEnable()
    {
        SetUI();
    }

    private void SetUI()
    {
        int index = (int)GameManager.Instance.GetEndingType();
        endingTxt.text = index.ToString() + "번째 엔딩이네여...";
    }

    public void GoMain()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.MAIN);
    }
}
