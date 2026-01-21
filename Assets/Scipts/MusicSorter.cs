using System;
using System.Collections.Generic;
using UnityEngine;

public static class MusicSorter
{
    [Serializable]
    public class SongInfo
    {
        public int OriginalIndex;
        public string Name;
        public float Duration;
        public string Genre;

        public SongInfo(int originalIndex, string name, float duration, string genre)
        {
            OriginalIndex = originalIndex;
            Name = name;
            Duration = duration;
            Genre = genre;
        }
    }

    // 从 clips 构建信息（Genre 这里先用 clip.name 规则简单推断；你也可以以后做真正的标签表）
    public static List<SongInfo> BuildSongInfos(IList<AudioClip> clips)
    {
        var infos = new List<SongInfo>();
        if (clips == null) return infos;

        for (int i = 0; i < clips.Count; i++)
        {
            var c = clips[i];
            if (c == null) continue;

            string name = c.name;
            float dur = c.length;

            // 简单 genre 规则：名字里包含关键词就归类，否则 "Unknown"
            string lower = name.ToLowerInvariant();
            string genre =
                lower.Contains("rock") ? "Rock" :
                lower.Contains("elect") ? "Electronic" :
                lower.Contains("dram") ? "Dramatic" :
                lower.Contains("eth") ? "Ethereal" :
                "Unknown";

            infos.Add(new SongInfo(i, name, dur, genre));
        }

        return infos;
    }

    // BubbleSort：按 Name 排序
    public static void BubbleSortByName(List<SongInfo> infos)
    {
        if (infos == null) return;

        int n = infos.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (string.Compare(infos[j].Name, infos[j + 1].Name, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    var tmp = infos[j];
                    infos[j] = infos[j + 1];
                    infos[j + 1] = tmp;
                }
            }
        }
    }

    // ExchangeSort：按 Duration 从短到长
    public static void ExchangeSortByDuration(List<SongInfo> infos)
    {
        if (infos == null) return;

        int n = infos.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (infos[i].Duration > infos[j].Duration)
                {
                    var tmp = infos[i];
                    infos[i] = infos[j];
                    infos[j] = tmp;
                }
            }
        }
    }

    // 组合排序：Genre 优先，再 Duration
    public static void SortByGenreThenDuration(List<SongInfo> infos)
    {
        if (infos == null) return;

        infos.Sort((a, b) =>
        {
            int g = string.Compare(a.Genre, b.Genre, StringComparison.OrdinalIgnoreCase);
            if (g != 0) return g;
            return a.Duration.CompareTo(b.Duration);
        });
    }
}
