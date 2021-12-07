using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintAside : MonoBehaviour
{
    public Text testTxt;

    void Start()
    {
        string text = "테스트 테스트 테스트 테스트 테스트 테스트 테스트 테스트";
        Print(text);
    }

    public void Print(string _text)
    {
        StartCoroutine(PrintLikeKeyboard(_text));
    }

    IEnumerator PrintLikeKeyboard(string _text)
    {
        for (int i = 0; i < _text.Length + 1; i++)
        {
            yield return new WaitForSeconds(0.1f);

            testTxt.text = _text.Substring(0, i);
        }

        StartCoroutine(DeleteString(_text));
    }

    IEnumerator DeleteString(string _text)
    {
        for (int i = _text.Length; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.05f);

            testTxt.text = _text.Substring(0, i);
        }
    }
}