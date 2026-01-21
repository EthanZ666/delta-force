using System;
using System.Collections.Generic;
using UnityEngine;

public static class MusicSearcher
{
    /// <summary>
    /// 把 clips 按名字 A->Z 排序，返回排序后的名字列表；
    /// 同时输出 sortedToOriginal：sortedIndex -> originalIndex
    /// </summary>
    public static List<string> GetSortedSongNames(AudioClip[] clips, out List<int> sortedToOriginal)
    {
        sortedToOriginal = new List<int>();
        var names = new List<string>();

        if (clips == null || clips.Length == 0) return names;

        // collect valid
        var temp = new List<(string name, int originalIndex)>();
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null) continue;
            temp.Add((clips[i].name, i));
        }

        // sort A->Z
        temp.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

        // output
        for (int i = 0; i < temp.Count; i++)
        {
            names.Add(temp[i].name);
            sortedToOriginal.Add(temp[i].originalIndex);
        }

        return names;
    }

    /// <summary>
    /// 在 names 里按 query 过滤（大小写不敏感），返回过滤后的 names；
    /// 同时输出 filteredToOriginal：filteredIndex -> originalIndex（原names的index）
    /// </summary>
    public static List<string> FilterNames(List<string> names, string query, out List<int> filteredToOriginal)
    {
        filteredToOriginal = new List<int>();
        var outNames = new List<string>();

        if (names == null || names.Count == 0) return outNames;

        if (string.IsNullOrWhiteSpace(query))
        {
            // no filter
            for (int i = 0; i < names.Count; i++)
            {
                outNames.Add(names[i]);
                filteredToOriginal.Add(i);
            }
            return outNames;
        }

        string q = query.Trim();

        for (int i = 0; i < names.Count; i++)
        {
            string n = names[i] ?? "";
            if (n.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                outNames.Add(n);
                filteredToOriginal.Add(i);
            }
        }

        return outNames;
    }
}
