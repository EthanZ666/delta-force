using System;
using System.Collections.Generic;

public static class MusicSorter
{
    public enum SortMode
    {
        Original,
        AtoZ,
        ZtoA
    }

    /// <summary>
    /// 给定 names，返回一个 indices 列表：
    /// indices[newIndex] = oldIndex
    /// </summary>
    public static List<int> GetSortedIndices(IReadOnlyList<string> names, SortMode mode)
    {
        var indices = new List<int>();
        if (names == null || names.Count == 0) return indices;

        for (int i = 0; i < names.Count; i++) indices.Add(i);

        if (mode == SortMode.Original) return indices;

        indices.Sort((i, j) =>
        {
            string a = names[i] ?? "";
            string b = names[j] ?? "";
            int cmp = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            return mode == SortMode.AtoZ ? cmp : -cmp;
        });

        return indices;
    }

    /// <summary>
    /// 用 indices 对 names 重排，返回新列表（不改原列表）
    /// </summary>
    public static List<string> ApplyOrder(IReadOnlyList<string> names, IReadOnlyList<int> indices)
    {
        var outNames = new List<string>();
        if (names == null || indices == null) return outNames;

        for (int k = 0; k < indices.Count; k++)
        {
            int old = indices[k];
            if (old >= 0 && old < names.Count) outNames.Add(names[old]);
        }
        return outNames;
    }
}
