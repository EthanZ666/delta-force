using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SettingScene : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenuScene";

    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC  = "volume_music";
    private const string PREF_ON     = "music_on";
    private const string PREF_INDEX  = "music_index";

    [Header("UI (optional, auto-find if empty)")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Button musicOnOffButton;
    public GameObject musicCheckmark;
    public Button backButton;

    // 你现在没有 InputField 也可以：留空就不启用搜索
    public InputField searchInput;          // 旧 UI
    public TMP_InputField tmpSearchInput;   // TMP UI（推荐）

    [Header("Auto-find Names")]
    public string masterSliderName = "Master Volume's Slider";
    public string musicSliderName  = "Music Volume's Slider";
    public string musicButtonName  = "Music On/Off Button";
    public string musicCheckmarkName = "Checkmark";
    public string backButtonName = "ReturnButton";
    public string searchInputName = "Search Input";
    public string selectMusicMenuName = "Select Music Menu";
    public string templateName = "Template";
    public string viewportName = "Viewport";
    public string contentName = "Content";

    private bool _musicOn = true;
    private Transform _contentTransform;
    private Transform _templateTransform;

    private void Awake()
    {
        AutoFindUIIfNeeded();
        WireUI();
        LoadAndApply();
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            GoBack();
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) BuildMusicListSortedByName();
        if (Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame) BuildMusicListSortedByDuration();
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame) BuildMusicListSortedByGenre();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) GoBack();
        if (Input.GetKeyDown(KeyCode.N)) BuildMusicListSortedByName();
        if (Input.GetKeyDown(KeyCode.D)) BuildMusicListSortedByDuration();
        if (Input.GetKeyDown(KeyCode.G)) BuildMusicListSortedByGenre();
#endif
    }

    private void AutoFindUIIfNeeded()
    {
        if (masterSlider == null) masterSlider = FindByNameInScene<Slider>(masterSliderName);
        if (musicSlider  == null) musicSlider  = FindByNameInScene<Slider>(musicSliderName);
        if (musicOnOffButton == null) musicOnOffButton = FindByNameInScene<Button>(musicButtonName);
        if (backButton == null) backButton = FindByNameInScene<Button>(backButtonName);

        if (musicCheckmark == null && musicOnOffButton != null)
        {
            var t = musicOnOffButton.transform.Find(musicCheckmarkName);
            if (t != null) musicCheckmark = t.gameObject;
        }

        if (searchInput == null) searchInput = FindByNameInScene<InputField>(searchInputName);
        if (tmpSearchInput == null) tmpSearchInput = FindByNameInScene<TMP_InputField>(searchInputName);

        var selectMenu = FindByNameInScene<Transform>(selectMusicMenuName);
        if (selectMenu != null)
        {
            var tTemplate = selectMenu.Find(templateName);
            if (tTemplate != null)
            {
                _templateTransform = tTemplate;
                tTemplate.gameObject.SetActive(false);
            }

            var viewport = selectMenu.Find(viewportName);
            if (viewport != null)
            {
                var content = viewport.Find(contentName);
                if (content != null) _contentTransform = content;
            }
        }
    }

    private void WireUI()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
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

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
            backButton.onClick.AddListener(GoBack);
        }

        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(OnSearchTextChanged);
            searchInput.onValueChanged.AddListener(OnSearchTextChanged);
        }

        if (tmpSearchInput != null)
        {
            tmpSearchInput.onValueChanged.RemoveListener(OnSearchTextChanged);
            tmpSearchInput.onValueChanged.AddListener(OnSearchTextChanged);
        }
    }

    private void LoadAndApply()
    {
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 1f));
        AudioListener.volume = master;
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);

        float musicVol = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 1f));
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicVol);

        _musicOn = PlayerPrefs.GetInt(PREF_ON, 1) == 1;
        if (musicCheckmark != null) musicCheckmark.SetActive(_musicOn);

        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.SetMusicVolume(musicVol);
            MusicPlayer.Instance.SetMusicOn(_musicOn);

            int idx = Mathf.Clamp(PlayerPrefs.GetInt(PREF_INDEX, 0), 0, MusicPlayer.Instance.SongCount - 1);
            MusicPlayer.Instance.PlayIndex(idx);
        }
        else
        {
            Debug.LogWarning("[SettingScene] MusicPlayer.Instance is null. (请确保场景里只有一个 MusicPlayer，并且它存在于运行时)");
        }

        BuildMusicListSortedByName();
        ClearSearchInputWithoutNotify();
    }

    private void OnMasterVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(PREF_MASTER, value);
        PlayerPrefs.Save();
    }

    private void OnMusicVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PREF_MUSIC, value);
        PlayerPrefs.Save();

        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicVolume(value);
    }

    private void ToggleMusicOnOff()
    {
        _musicOn = !_musicOn;
        PlayerPrefs.SetInt(PREF_ON, _musicOn ? 1 : 0);
        PlayerPrefs.Save();

        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicOn(_musicOn);

        if (musicCheckmark != null)
            musicCheckmark.SetActive(_musicOn);
    }

    private void OnSearchTextChanged(string query)
    {
        if (_contentTransform == null) return;

        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (Transform child in _contentTransform)
                child.gameObject.SetActive(true);
            return;
        }

        query = query.Trim().ToLowerInvariant();

        foreach (Transform child in _contentTransform)
        {
            string label = GetLabelText(child);
            bool hit = !string.IsNullOrEmpty(label) && label.ToLowerInvariant().Contains(query);
            child.gameObject.SetActive(hit);
        }
    }

    private void GoBack()
    {
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogError($"Scene not in Build Settings: {mainMenuSceneName}");
    }

    private void BuildMusicListSortedByName()
    {
        if (MusicPlayer.Instance == null || _contentTransform == null || _templateTransform == null) return;
        if (MusicPlayer.Instance.songs == null || MusicPlayer.Instance.songs.Count == 0) return;

        ClearMusicButtons();

        var infos = MusicSorter.BuildSongInfos(MusicPlayer.Instance.songs);
        MusicSorter.BubbleSortByName(infos);
        CreateMusicButtonsFromInfos(infos);

        ClearSearchInputWithoutNotify();
    }

    private void BuildMusicListSortedByDuration()
    {
        if (MusicPlayer.Instance == null || _contentTransform == null || _templateTransform == null) return;
        if (MusicPlayer.Instance.songs == null || MusicPlayer.Instance.songs.Count == 0) return;

        ClearMusicButtons();

        var infos = MusicSorter.BuildSongInfos(MusicPlayer.Instance.songs);
        MusicSorter.ExchangeSortByDuration(infos);
        CreateMusicButtonsFromInfos(infos);

        ClearSearchInputWithoutNotify();
    }

    private void BuildMusicListSortedByGenre()
    {
        if (MusicPlayer.Instance == null || _contentTransform == null || _templateTransform == null) return;
        if (MusicPlayer.Instance.songs == null || MusicPlayer.Instance.songs.Count == 0) return;

        ClearMusicButtons();

        var infos = MusicSorter.BuildSongInfos(MusicPlayer.Instance.songs);
        MusicSorter.SortByGenreThenDuration(infos);
        CreateMusicButtonsFromInfos(infos);

        ClearSearchInputWithoutNotify();
    }

    private void ClearMusicButtons()
    {
        if (_contentTransform == null) return;
        for (int i = _contentTransform.childCount - 1; i >= 0; i--)
            Destroy(_contentTransform.GetChild(i).gameObject);
    }

    private void CreateMusicButtonsFromInfos(List<MusicSorter.SongInfo> infos)
    {
        if (_templateTransform == null || _contentTransform == null) return;

        for (int i = 0; i < infos.Count; i++)
        {
            int originalIndex = infos[i].OriginalIndex;
            string name = infos[i].Name;

            var btnObj = Instantiate(_templateTransform.gameObject, _contentTransform);
            btnObj.SetActive(true);
            btnObj.name = $"Song_{originalIndex}";

            SetLabelText(btnObj.transform, name);

            var buttonComp = btnObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                int idx = originalIndex;
                buttonComp.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt(PREF_INDEX, idx);
                    PlayerPrefs.Save();

                    if (MusicPlayer.Instance != null)
                        MusicPlayer.Instance.PlayIndex(idx);
                });
            }
        }
    }

    private void ClearSearchInputWithoutNotify()
    {
        if (searchInput != null) searchInput.SetTextWithoutNotify("");
        if (tmpSearchInput != null) tmpSearchInput.SetTextWithoutNotify("");
    }

    private static string GetLabelText(Transform root)
    {
        if (root == null) return "";

        var tmp = root.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) return tmp.text;

        var t = root.GetComponentInChildren<Text>(true);
        if (t != null) return t.text;

        return "";
    }

    private static void SetLabelText(Transform root, string text)
    {
        if (root == null) return;

        var tmp = root.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) { tmp.text = text; return; }

        var t = root.GetComponentInChildren<Text>(true);
        if (t != null) { t.text = text; return; }
    }

    private static T FindByNameInScene<T>(string name) where T : Component
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var all = Resources.FindObjectsOfTypeAll<Transform>();
        var activeScene = SceneManager.GetActiveScene();

        for (int i = 0; i < all.Length; i++)
        {
            var tr = all[i];
            if (tr == null || tr.name != name) continue;

            var go = tr.gameObject;
            if (!go.scene.IsValid() || go.scene != activeScene) continue;

            return go.GetComponent<T>();
        }

        return null;
    }
}
