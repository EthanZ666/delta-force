using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Optional UI Reference (TMP_Dropdown)")]
    public TMP_Dropdown dropdown;

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
        // 如果已经有实例，并且不是自己 -> 删掉自己
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // 防止从别的场景带来的“暂停/静音”
        AudioListener.pause = false;
        AudioListener.volume = 1f;
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

        // 可选：如果你在 SettingScene 里有下拉框，就自动填充+绑定
        if (dropdown != null)
        {
            BindDropdown(dropdown);
            // 让UI显示当前保存的index（不触发播放）
            dropdown.SetValueWithoutNotify(_currentIndex);
        }

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

    /// <summary>
    /// 绑定下拉菜单：自动填充选项 + 监听 index 改变
    /// </summary>
    public void BindDropdown(TMP_Dropdown dd)
    {
        dropdown = dd;
        if (dropdown == null) return;

        dropdown.ClearOptions();

        var opts = new List<string>();
        if (songs != null)
        {
            for (int i = 0; i < songs.Count; i++)
            {
                opts.Add(songs[i] == null ? $"Song {i}" : songs[i].name);
            }
        }
        dropdown.AddOptions(opts);

        // 防止重复绑定（例如多次进入SettingScene）
        dropdown.onValueChanged.RemoveListener(PlayByIndex);
        dropdown.onValueChanged.AddListener(PlayByIndex);
    }

    // ===== UnityEvent target for TMP_Dropdown.OnValueChanged(Int32) =====
    public void PlayByIndex(int index)
    {
        // 这里不要用 text/name 了，直接用 index 播放，最稳定
        PlayIndex(index);

        // 保证UI不乱（可选）
        if (dropdown != null && dropdown.value != index)
            dropdown.SetValueWithoutNotify(index);
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

        // —— 自动修正音量的关键逻辑在这里 ——
        float vol = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, -1f);

        // 第一次运行（-1）或之前被存成 0 → 自动设为 1
        if (vol < 0.05f)
        {
            vol = 1f;
            PlayerPrefs.SetFloat(PREF_MUSIC_VOL, vol);
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
