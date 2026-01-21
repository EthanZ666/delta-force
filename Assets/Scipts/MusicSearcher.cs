using System;
using System.Collections.Generic;

public static class MusicSearcher
{
    // 返回满足关键词的索引（不改 UI，只做纯算法）
    public static List<int> LinearSearchByKeyword(List<MusicSorter.SongInfo> infos, string keyword)
    {
        var result = new List<int>();
        if (infos == null) return result;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            for (int i = 0; i < infos.Count; i++) result.Add(i);
            return result;
        }

        keyword = keyword.Trim().ToLowerInvariant();

        for (int i = 0; i < infos.Count; i++)
        {
            var name = infos[i].Name == null ? "" : infos[i].Name.ToLowerInvariant();
            if (name.Contains(keyword))
                result.Add(i);
        }

        return result;
    }
}
