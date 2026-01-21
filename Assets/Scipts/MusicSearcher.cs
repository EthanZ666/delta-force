using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供对歌曲名称列表进行排序和查找的工具类。
/// </summary>
public static class MusicSearcher
{
    /// <summary>
    /// 返回按名称(A-Z)排序后的名称列表，并输出排序后索引到原始索引的映射。
    /// </summary>
    public static List<string> GetSortedSongNames(AudioClip[] clips, out List<int> sortedToOriginal)
    {
        sortedToOriginal = new List<int>();
        var names = new List<string>();
        if (clips == null || clips.Length == 0)
            return names;

        var pairs = new List<(string name, int originalIndex)>(clips.Length);
        for (int i = 0; i < clips.Length; i++)
        {
            string n = clips[i] != null ? clips[i].name : $"(MissingClip_{i})";
            pairs.Add((n, i));
        }
        pairs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        for (int i = 0; i < pairs.Count; i++)
        {
            names.Add(pairs[i].name);
            sortedToOriginal.Add(pairs[i].originalIndex);
        }
        return names;
    }

    /// <summary>
    /// 在排序列表中按关键字查找第一个包含匹配的项（忽略大小写），返回索引或 -1。
    /// </summary>
    public static int FindContains(List<string> names, string keyword)
    {
        if (names == null || names.Count == 0) return -1;
        if (string.IsNullOrWhiteSpace(keyword)) return -1;
        keyword = keyword.Trim();
        for (int i = 0; i < names.Count; i++)
        {
            if (names[i] != null && names[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 对已排序名称列表进行二分查找（完全匹配，忽略大小写），返回索引或 -1。
    /// </summary>
    public static int BinarySearchExact(List<string> sortedNames, string target)
    {
        if (sortedNames == null || sortedNames.Count == 0) return -1;
        if (string.IsNullOrWhiteSpace(target)) return -1;
        int lo = 0;
        int hi = sortedNames.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            int cmp = string.Compare(sortedNames[mid], target, StringComparison.OrdinalIgnoreCase);
            if (cmp == 0) return mid;
            if (cmp < 0) lo = mid + 1; else hi = mid - 1;
        }
        return -1;
    }
}
