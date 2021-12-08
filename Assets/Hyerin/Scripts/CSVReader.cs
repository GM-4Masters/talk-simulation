using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<ChatData> Read(string file)
    {
        var list = new List<ChatData>();
        TextAsset data = Resources.Load(file) as TextAsset;

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 1) continue;

            var entry = new ChatData();
            if (values[0] != "") entry.index = (int)Convert(values[0]);
            //entry.index = (int)Convert(values[0]);
            entry.chatroom = (string)Convert(values[1]);
            if (values[2]!="") entry.dt = (float)Convert(values[2]);
            //entry.dt = (float)Convert(values[2]);
            entry.date = (string)Convert(values[3]);
            entry.time = (string)Convert(values[4]);
            entry.character = (string)Convert(values[5]);
            if (entry.character == "ÀÔÀå" || entry.character == "ÅðÀå")
            {
                entry.enterNum = (int)Convert(values[6]);
            }
            else entry.text = (string)Convert(values[6]);

            list.Add(entry);
        }
        return list;
    }

    public static object Convert(string str)
    {
        string value = str;
        value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

        //value = value.Replace("<br>", "\n");
        //value = value.Replace("<c>", ",");

        if (value.Contains("unCheck"))
        {
            value = value.Substring(19);
        }

        value = value.Replace("<br>", "\n");

        object finalvalue = value;
        int n;
        float f;
        if (int.TryParse(value, out n))
        {
            finalvalue = n;
        }
        else if (float.TryParse(value, out f))
        {
            finalvalue = f;
        }
        return finalvalue;
    }
}
