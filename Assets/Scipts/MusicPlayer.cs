using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Audio Source (auto-get if empty)")]
    public AudioSource musicSource;

    [Header("Songs (drag AudioClips here)")]
    public List<AudioClip> songs = new List<AudioClip>();

    // PlayerPrefs Keys (和你 SettingScene 保持一致)
    private const string PREF_MUSIC_ON   = "music_on";
    private const string PREF_MUSIC_VOL  = "volume_music";
    private const string PREF_SONG_INDEX = "music_index";

    private int _currentIndex = 0;
    private bool _musicOn = true;

    public int CurrentIndex => _currentIndex;
    public bool MusicOn => _musicOn;
    public int SongCount => songs == null ? 0 : songs.Count;

    public event Action<int, AudioClip> SongChanged;

    private void Awake()
    {
        // ===== Singleton =====
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 如果你希望跨场景继续播放，打开这行
        DontDestroyOnLoad(gameObject);

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

        if (songs == null) songs = new List<AudioClip>();
        if (songs.Count == 0)
        {
            Debug.LogWarning("[MusicPlayer] songs list is empty.");
            return;
        }

        _currentIndex = Mathf.Clamp(_currentIndex, 0, songs.Count - 1);

        ApplyVolume();
        ApplyOnOff();

        if (_musicOn)
            PlayIndex(_currentIndex);
    }

    // ===== Public API =====
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

    public void PlayIndex(int index)
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

        musicSource.clip = clip;

        if (_musicOn)
            musicSource.Play();

        SavePrefs();

        SongChanged?.Invoke(_currentIndex, clip);
        Debug.Log($"[MusicPlayer] Playing: {clip.name} (index {_currentIndex})");
    }

    public void SetMusicOn(bool on)
    {
        _musicOn = on;
        ApplyOnOff();
        SavePrefs();
    }

    public void SetMusicVolume(float v01)
    {
        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, Mathf.Clamp01(v01));
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public float GetMusicVolume()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 1f));
    }

    // ===== Internal =====
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
            if (musicSource.clip == null && songs != null && songs.Count > 0)
                musicSource.clip = songs[Mathf.Clamp(_currentIndex, 0, songs.Count - 1)];

            if (musicSource.clip != null && !musicSource.isPlaying)
                musicSource.Play();
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
