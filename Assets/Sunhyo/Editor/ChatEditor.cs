using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChatManager))]
public class ChatEditor : Editor
{
    ChatManager chatManager;
    string text;

    void OnEnable()
    {
        chatManager = target as ChatManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        text = EditorGUILayout.TextArea(text);
        
        if(GUILayout.Button("������", GUILayout.Width(60)) && text.Trim() != "")
        {
            chatManager.Chat(true, text, "���� 7:00");
            text = "";
            GUI.FocusControl(null);
        }

        if(GUILayout.Button("�ޱ�", GUILayout.Width(60)) && text.Trim() != "")
        {
            chatManager.Chat(false, text, "���� 8:32", "�Ϳ��� ������ Ǫ", "4", "", "�� �Ǵ��� ���ô�.txt");
            text = "";
            GUI.FocusControl(null);
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
