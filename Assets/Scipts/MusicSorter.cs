using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供按名称/时长/类型对歌曲列表进行排序的工具类。
/// </summary>
public static class MusicSorter
{
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

    // 根据 AudioClip 数组构建 SongInfo 列表
    public static List<SongInfo> BuildSongInfos(AudioClip[] clips)
    {
        var list = new List<SongInfo>();
        if (clips == null) return list;
        for (int i = 0; i < clips.Length; i++)
        {
            var c = clips[i];
            string name = c != null ? c.name : $"(MissingClip_{i})";
            float dur = c != null ? c.length : 0f;
            string genre = InferGenreFromName(name);
            list.Add(new SongInfo(i, name, dur, genre));
        }
        return list;
    }

    /// <summary>冒泡按名称（A-Z）排序。</summary>
    public static void BubbleSortByName(List<SongInfo> songs)
    {
        if (songs == null) return;
        for (int i = 0; i < songs.Count - 1; i++)
        {
            for (int j = 0; j < songs.Count - 1 - i; j++)
            {
                if (string.Compare(songs[j].Name, songs[j + 1].Name, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    var tmp = songs[j];
                    songs[j] = songs[j + 1];
                    songs[j + 1] = tmp;
                }
            }
        }
    }

    /// <summary>交换排序按时长（短到长）排序。</summary>
    public static void ExchangeSortByDuration(List<SongInfo> songs)
    {
        if (songs == null) return;
        for (int i = 0; i < songs.Count - 1; i++)
        {
            for (int j = i + 1; j < songs.Count; j++)
            {
                if (songs[i].Duration > songs[j].Duration)
                {
                    var tmp = songs[i];
                    songs[i] = songs[j];
                    songs[j] = tmp;
                }
            }
        }
    }

    /// <summary>先按类型字母排序，再按时长排序（类型相同的情况下）。</summary>
    public static void SortByGenreThenDuration(List<SongInfo> songs)
    {
        if (songs == null) return;
        songs.Sort((a, b) =>
        {
            int g = string.Compare(a.Genre, b.Genre, StringComparison.OrdinalIgnoreCase);
            if (g != 0) return g;
            return a.Duration.CompareTo(b.Duration);
        });
    }

    // 简单地从名称推测类型（示例用，可根据实际名称调整）
    public static string InferGenreFromName(string songName)
    {
        if (string.IsNullOrWhiteSpace(songName)) return "Misc";
        string n = songName.ToLowerInvariant();
        if (n.Contains("battle") || n.Contains("combat") || n.Contains("war"))
            return "Action";
        if (n.Contains("calm") || n.Contains("menu") || n.Contains("lobby"))
            return "Chill";
        if (n.Contains("sad") || n.Contains("slow"))
            return "Sad";
        return "Misc";
    }
}
