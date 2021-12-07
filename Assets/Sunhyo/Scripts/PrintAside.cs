using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Talk : MonoBehaviour
{
    // ����� �ؽ�Ʈ ������ ���� �߰��ϴ� ����� Resource ����� �ٲ�� ��
    public TextAsset txt;

    string[,] Sentence;
    int lineSize, rowSize;

    private State state = State.NotInitialized;
    private List<string> script = new List<string>();

    public int currentIdx = 0;
    
    [SerializeField]
    Text uiText;

    enum State
    {
        NotInitialized,
        Playing,
        PlayingSkipping,
        Completed,
    }

    IEnumerator Print(string script)
    {
        for (int i = 0; i < script.Length + 1; i++)
        {
            yield return new WaitForSeconds(0.1f);
            if (state == State.PlayingSkipping)
            {
                uiText.text = script;
                state = State.Playing;
                break;
            }
            uiText.text = script.Substring(0, i);
        }
        
        while(state != State.PlayingSkipping)
            yield return new WaitForSeconds(0.1f);
        state = State.Playing;
    }
}