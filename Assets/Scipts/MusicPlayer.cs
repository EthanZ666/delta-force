using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Header("Audio Source (auto-get if empty)")]
    public AudioSource musicSource;

    [Header("Songs (drag AudioClips here)")]
    public List<AudioClip> songs = new List<AudioClip>();

    // PlayerPrefs Keys
    private const string PREF_MUSIC_ON = "music_on";
    private const string PREF_MUSIC_VOL = "volume_music";
    private const string PREF_SONG_INDEX = "music_song_index";

    private int _currentIndex = 0;
    private bool _musicOn = true;

    public int CurrentIndex => _currentIndex;
    public bool MusicOn => _musicOn;

    public event Action<int, AudioClip> SongChanged;

    private void Awake()
    {
        // 如果你希望跨场景继续播放就打开这行
        // DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
        LoadPrefs();

        // 没歌就不做
        if (songs == null) songs = new List<AudioClip>();
        if (songs.Count == 0)
        {
            Debug.LogWarning("[MusicPlayer] songs list is empty.");
            return;
        }

        // clamp index
        _currentIndex = Mathf.Clamp(_currentIndex, 0, songs.Count - 1);

        // apply volume + on/off
        ApplyVolume();
        ApplyOnOff();

        // 如果开着音乐就播放上次那首
        if (_musicOn)
        {
            PlaySong(_currentIndex);
        }
    }

    // =========================
    // Public API
    // =========================
    public int SongCount()
    {
        return songs == null ? 0 : songs.Count;
    }

    public AudioClip GetSong(int index)
    {
        if (songs == null || songs.Count == 0) return null;
        if (index < 0 || index >= songs.Count) return null;
        return songs[index];
    }

    public string GetSongName(int index)
    {
        var c = GetSong(index);
        return c == null ? "" : c.name;
    }

    public List<string> GetAllSongNames()
    {
        var list = new List<string>();
        if (songs == null) return list;

        for (int i = 0; i < songs.Count; i++)
        {
            if (songs[i] != null) list.Add(songs[i].name);
        }
        return list;
    }

    public void PlaySong(int index)
    {
        if (songs == null || songs.Count == 0) return;
        index = Mathf.Clamp(index, 0, songs.Count - 1);

        var clip = songs[index];
        if (clip == null)
        {
            Debug.LogWarning($"[MusicPlayer] Song at index {index} is null.");
            return;
        }

        _currentIndex = index;

        if (musicSource == null) return;

        musicSource.clip = clip;

        if (_musicOn)
        {
            musicSource.Play();
        }

        SavePrefs();

        SongChanged?.Invoke(_currentIndex, clip);
        Debug.Log($"[MusicPlayer] Playing: {clip.name} (index {_currentIndex})");
    }

    public void PlaySongByName(string songName)
    {
        if (string.IsNullOrWhiteSpace(songName)) return;
        if (songs == null) return;

        for (int i = 0; i < songs.Count; i++)
        {
            var c = songs[i];
            if (c != null && string.Equals(c.name, songName, StringComparison.OrdinalIgnoreCase))
            {
                PlaySong(i);
                return;
            }
        }

        Debug.LogWarning($"[MusicPlayer] Song not found: {songName}");
    }

    public void NextSong()
    {
        if (songs == null || songs.Count == 0) return;
        int next = (_currentIndex + 1) % songs.Count;
        PlaySong(next);
    }

    public void PrevSong()
    {
        if (songs == null || songs.Count == 0) return;
        int prev = _currentIndex - 1;
        if (prev < 0) prev = songs.Count - 1;
        PlaySong(prev);
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetMusicOn(bool on)
    {
        _musicOn = on;
        ApplyOnOff();
        SavePrefs();
    }

    public void ToggleMusicOn()
    {
        SetMusicOn(!_musicOn);
    }

    public void SetMusicVolume(float v01)
    {
        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, Mathf.Clamp01(v01));
        PlayerPrefs.Save();

        ApplyVolume();
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 1f);
    }

    // =========================
    // Internal
    // =========================
    private void ApplyVolume()
    {
        if (musicSource == null) return;
        musicSource.volume = GetMusicVolume();
    }

    private void ApplyOnOff()
    {
        if (musicSource == null) return;

        if (_musicOn)
        {
            // 有clip才播
            if (musicSource.clip == null && songs != null && songs.Count > 0)
            {
                musicSource.clip = songs[Mathf.Clamp(_currentIndex, 0, songs.Count - 1)];
            }

            if (musicSource.clip != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.isPlaying) musicSource.Stop();
        }
    }

    private void LoadPrefs()
    {
        _musicOn = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;
        _currentIndex = PlayerPrefs.GetInt(PREF_SONG_INDEX, 0);

        // volume key 可能你用的就是 volume_music
        if (!PlayerPrefs.HasKey(PREF_MUSIC_VOL))
        {
            PlayerPrefs.SetFloat(PREF_MUSIC_VOL, 1f);
            PlayerPrefs.Save();
        }
    }

    private void SavePrefs()
    {
        PlayerPrefs.SetInt(PREF_MUSIC_ON, _musicOn ? 1 : 0);
        PlayerPrefs.SetInt(PREF_SONG_INDEX, _currentIndex);
        PlayerPrefs.Save();
    }
}
