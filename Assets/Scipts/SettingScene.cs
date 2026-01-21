using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// SettingsScene controller (safe version: does NOT require a specific MusicPlayer API).
/// - Master volume slider -> AudioListener.volume + PlayerPrefs "volume_master"
/// - Music volume slider -> tries to set MusicPlayer volume OR AudioSource.volume + PlayerPrefs "volume_music"
/// - Music ON/OFF button -> tries to mute/stop MusicPlayer OR AudioSource + PlayerPrefs "music_on"
/// - Music dropdown (optional) -> tries to tell MusicPlayer to play selected clip OR plays on AudioSource directly
///
/// It auto-finds UI by hierarchy names if you don't drag references.
/// </summary>
public class SettingScene : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenuScene";

    // ===== PlayerPrefs Keys (must match GameHotkeys) =====
    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC = "volume_music";
    private const string PREF_MUSIC_ON = "music_on";
    private const string PREF_MUSIC_INDEX = "music_index";

    // ===== UI References =====
    [Header("UI (optional, auto-find if empty)")]
    public Slider masterSlider;
    public Slider musicSlider;

    [Tooltip("Button GameObject: Music On/Off Button")]
    public Button musicOnOffButton;

    [Tooltip("Child GameObject under Music On/Off Button: Checkmark")]
    public GameObject musicCheckmark;

    [Tooltip("TMP_Dropdown under Select Music Menu (optional)")]
    public TMP_Dropdown musicDropdown;

    [Tooltip("Optional Back button to return Main Menu")]
    public Button backButton;

    // ===== Auto-find Names (match your hierarchy) =====
    [Header("Auto-find Names")]
    public string masterSliderName = "Master Volume's Slider";
    public string musicSliderName = "Music Volume's Slider";
    public string musicButtonName = "Music On/Off Button";
    public string musicCheckmarkName = "Checkmark";
    public string dropdownName = "Select Music Menu";
    public string backButtonName = "ReturnButton";

    // ===== Runtime =====
    private bool _musicOn = true;

    // music source (fallback)
    private AudioSource _audioSource;

    // music player (optional, via reflection)
    private Component _musicPlayerComp;

    // dropdown mapping
    private List<int> _sortedToOriginal = new List<int>();
    private AudioClip[] _clips;

    private void Awake()
    {
        AutoFindUIIfNeeded();

        // Try find a MusicPlayer component (any API) + AudioSource
        FindMusicComponents();

        WireUI();
        LoadAndApply();
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            GoBack();
#else
        if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
#endif
    }

    // =========================
    // Setup
    // =========================
    private void AutoFindUIIfNeeded()
    {
        if (masterSlider == null) masterSlider = FindByNameInScene<Slider>(masterSliderName);
        if (musicSlider == null) musicSlider = FindByNameInScene<Slider>(musicSliderName);
        if (musicOnOffButton == null) musicOnOffButton = FindByNameInScene<Button>(musicButtonName);
        if (musicDropdown == null) musicDropdown = FindByNameInScene<TMP_Dropdown>(dropdownName);
        if (backButton == null) backButton = FindByNameInScene<Button>(backButtonName);

        if (musicCheckmark == null && musicOnOffButton != null)
        {
            var t = musicOnOffButton.transform.Find(musicCheckmarkName);
            if (t != null) musicCheckmark = t.gameObject;
        }
    }

    private void FindMusicComponents()
    {
        // 1) find object named MusicPlayer in scene or DontDestroyOnLoad
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            var go = all[i];
            if (go == null) continue;
            if (!go.scene.IsValid()) continue; // skip assets/prefabs
            if (go.name != "MusicPlayer") continue;

            _musicPlayerComp = go.GetComponent("MusicPlayer") as Component;
            _audioSource = go.GetComponent<AudioSource>();
            break;
        }

        // 2) if no "MusicPlayer" object, try any AudioSource in scene
        if (_audioSource == null)
        {
            _audioSource = UnityEngine.Object.FindFirstObjectByType<AudioSource>(FindObjectsInactive.Include);
        }

        // 3) try to get clips list from MusicPlayer (common patterns)
        _clips = TryGetClipsFromMusicPlayer(_musicPlayerComp);

        // 4) if still null, we can’t build dropdown from clips
        if (_clips == null || _clips.Length == 0)
        {
            _clips = Array.Empty<AudioClip>();
        }
    }

    private void WireUI()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (musicOnOffButton != null)
        {
            musicOnOffButton.onClick.RemoveListener(ToggleMusicOnOff);
            musicOnOffButton.onClick.AddListener(ToggleMusicOnOff);
        }

        if (musicDropdown != null)
        {
            musicDropdown.onValueChanged.RemoveListener(OnMusicDropdownChanged);
            musicDropdown.onValueChanged.AddListener(OnMusicDropdownChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
            backButton.onClick.AddListener(GoBack);
        }
    }

    private void LoadAndApply()
    {
        // Master volume
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 1f));
        AudioListener.volume = master;
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);

        // Music volume
        float musicV = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 1f));
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicV);

        // Music on/off
        _musicOn = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;
        ApplyMusicOnOffVisual();
        ApplyMusicState(musicV);

        // Dropdown
        if (musicDropdown != null)
        {
            BuildDropdownOptions();
            int savedIndex = PlayerPrefs.GetInt(PREF_MUSIC_INDEX, 0);
            savedIndex = Mathf.Clamp(savedIndex, 0, Mathf.Max(0, musicDropdown.options.Count - 1));
            musicDropdown.SetValueWithoutNotify(savedIndex);
        }
    }

    // =========================
    // UI handlers
    // =========================
    private void OnMasterChanged(float v)
    {
        v = Mathf.Clamp01(v);
        AudioListener.volume = v;

        PlayerPrefs.SetFloat(PREF_MASTER, v);
        PlayerPrefs.Save();
    }

    private void OnMusicVolumeChanged(float v)
    {
        v = Mathf.Clamp01(v);

        PlayerPrefs.SetFloat(PREF_MUSIC, v);
        PlayerPrefs.Save();

        ApplyMusicState(v);
    }

    private void ToggleMusicOnOff()
    {
        _musicOn = !_musicOn;

        PlayerPrefs.SetInt(PREF_MUSIC_ON, _musicOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicOnOffVisual();
        ApplyMusicState(PlayerPrefs.GetFloat(PREF_MUSIC, 1f));
    }

    private void ApplyMusicOnOffVisual()
    {
        if (musicCheckmark != null) musicCheckmark.SetActive(_musicOn);
    }

    private void OnMusicDropdownChanged(int dropdownIndex)
    {
        PlayerPrefs.SetInt(PREF_MUSIC_INDEX, dropdownIndex);
        PlayerPrefs.Save();

        int originalIndex = dropdownIndex;
        if (_sortedToOriginal != null && _sortedToOriginal.Count == _clips.Length && _clips.Length > 0)
            originalIndex = _sortedToOriginal[Mathf.Clamp(dropdownIndex, 0, _sortedToOriginal.Count - 1)];

        TryPlayIndex(originalIndex);
    }

    private void GoBack()
    {
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogError($"[SettingScene] Scene not in Build Settings: {mainMenuSceneName}");
    }

    // =========================
    // Music integration (reflection + fallback)
    // =========================
    private void ApplyMusicState(float musicVolume)
    {
        // Try call MusicPlayer API if it exists
        bool usedMusicPlayer = false;

        if (_musicPlayerComp != null)
        {
            usedMusicPlayer |= TryInvokeBool(_musicPlayerComp, "SetMusicOn", new object[] { _musicOn });
            usedMusicPlayer |= TryInvokeBool(_musicPlayerComp, "SetMuted", new object[] { !_musicOn });
            usedMusicPlayer |= TryInvokeBool(_musicPlayerComp, "SetMusicVolume", new object[] { musicVolume });
            usedMusicPlayer |= TryInvokeBool(_musicPlayerComp, "SetVolume", new object[] { musicVolume });
        }

        // Fallback: AudioSource
        if (!usedMusicPlayer && _audioSource != null)
        {
            _audioSource.volume = musicVolume;

            if (_musicOn)
            {
                if (!_audioSource.isPlaying) _audioSource.Play();
                _audioSource.mute = false;
            }
            else
            {
                _audioSource.mute = true;
            }
        }
    }

    private void TryPlayIndex(int originalIndex)
    {
        if (_clips == null || _clips.Length == 0) return;
        originalIndex = Mathf.Clamp(originalIndex, 0, _clips.Length - 1);

        // Try MusicPlayer methods
        if (_musicPlayerComp != null)
        {
            if (TryInvokeBool(_musicPlayerComp, "PlayByIndex", new object[] { originalIndex })) return;
            if (TryInvokeBool(_musicPlayerComp, "PlaySong", new object[] { originalIndex })) return;
            if (TryInvokeBool(_musicPlayerComp, "Play", new object[] { originalIndex })) return;
        }

        // Fallback: play on AudioSource
        if (_audioSource != null)
        {
            _audioSource.clip = _clips[originalIndex];
            if (_musicOn)
            {
                _audioSource.mute = false;
                _audioSource.Play();
            }
        }
    }

    private void BuildDropdownOptions()
    {
        musicDropdown.ClearOptions();
        _sortedToOriginal.Clear();

        if (_clips == null || _clips.Length == 0)
        {
            musicDropdown.AddOptions(new List<string> { "(No songs)" });
            return;
        }

        var sortedNames = MusicSearcher.GetSortedSongNames(_clips, out _sortedToOriginal);
        musicDropdown.AddOptions(sortedNames);
    }

    private static bool TryInvokeBool(Component comp, string methodName, object[] args)
    {
        try
        {
            var t = comp.GetType();
            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) return false;
            m.Invoke(comp, args);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static AudioClip[] TryGetClipsFromMusicPlayer(Component musicPlayerComp)
    {
        if (musicPlayerComp == null) return null;

        try
        {
            var t = musicPlayerComp.GetType();

            // common field/property names
            var fSongs = t.GetField("songs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fSongs != null)
            {
                var val = fSongs.GetValue(musicPlayerComp);
                if (val is AudioClip[] a1) return a1;
                if (val is List<AudioClip> l1) return l1.ToArray();
            }

            var pSongs = t.GetProperty("songs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pSongs != null)
            {
                var val = pSongs.GetValue(musicPlayerComp);
                if (val is AudioClip[] a2) return a2;
                if (val is List<AudioClip> l2) return l2.ToArray();
            }

            // method GetSongs()
            var mGet = t.GetMethod("GetSongs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (mGet != null)
            {
                var val = mGet.Invoke(musicPlayerComp, null);
                if (val is AudioClip[] a3) return a3;
                if (val is List<AudioClip> l3) return l3.ToArray();
            }
        }
        catch { }

        return null;
    }

    // =========================
    // Helpers
    // =========================
    private static T FindByNameInScene<T>(string name) where T : Component
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var all = Resources.FindObjectsOfTypeAll<Transform>();
        var activeScene = SceneManager.GetActiveScene();

        for (int i = 0; i < all.Length; i++)
        {
            var t = all[i];
            if (t == null) continue;
            if (t.name != name) continue;

            var go = t.gameObject;
            if (!go.scene.IsValid()) continue;
            if (go.scene != activeScene) continue;

            return go.GetComponent<T>();
        }

        return null;
    }
}
