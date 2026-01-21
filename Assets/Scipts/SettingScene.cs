using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SettingScene : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenuScene";

    // PlayerPrefs keys
    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC = "volume_music";
    private const string PREF_ON = "music_on";
    private const string PREF_INDEX = "music_index";

    [Header("UI (optional, auto-find if empty)")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Button musicOnOffButton;
    public GameObject musicCheckmark;
    public Button backButton;
    public InputField searchInput;

    [Header("Auto-find Names")]
    public string masterSliderName = "Master Volume's Slider";
    public string musicSliderName = "Music Volume's Slider";
    public string musicButtonName = "Music On/Off Button";
    public string musicCheckmarkName = "Checkmark";
    public string backButtonName = "ReturnButton";
    public string searchInputName = "Search Input";
    public string selectMusicMenuName = "Select Music Menu";
    public string templateName = "Template";
    public string viewportName = "Viewport";
    public string contentName = "Content";

    // Internal state
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
        if (Keyboard.current.nKey.wasPressedThisFrame) BuildMusicListSortedByName();
        if (Keyboard.current.dKey.wasPressedThisFrame) BuildMusicListSortedByDuration();
        if (Keyboard.current.gKey.wasPressedThisFrame) BuildMusicListSortedByGenre();
#else
        if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
        if (Input.GetKeyDown(KeyCode.N)) BuildMusicListSortedByName();
        if (Input.GetKeyDown(KeyCode.D)) BuildMusicListSortedByDuration();
        if (Input.GetKeyDown(KeyCode.G)) BuildMusicListSortedByGenre();
#endif
    }

    // 自动绑定 UI 控件
    private void AutoFindUIIfNeeded()
    {
        if (masterSlider == null)
            masterSlider = FindByNameInScene<Slider>(masterSliderName);
        if (musicSlider == null)
            musicSlider = FindByNameInScene<Slider>(musicSliderName);
        if (musicOnOffButton == null)
            musicOnOffButton = FindByNameInScene<Button>(musicButtonName);
        if (backButton == null)
            backButton = FindByNameInScene<Button>(backButtonName);

        if (musicCheckmark == null && musicOnOffButton != null)
        {
            var t = musicOnOffButton.transform.Find(musicCheckmarkName);
            if (t != null) musicCheckmark = t.gameObject;
        }

        if (searchInput == null)
            searchInput = FindByNameInScene<InputField>(searchInputName);

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
                if (content != null)
                    _contentTransform = content;
            }
        }
    }

    // 连接 UI 事件
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
    }

    // 读取并应用保存的设置
    private void LoadAndApply()
    {
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 1f));
        AudioListener.volume = master;
        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(master);

        float musicVol = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 1f));
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(musicVol);

        _musicOn = PlayerPrefs.GetInt(PREF_ON, 1) == 1;
        if (musicCheckmark != null)
            musicCheckmark.SetActive(_musicOn);
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicOn(_musicOn);

        BuildMusicListSortedByName();

        if (MusicPlayer.Instance != null)
        {
            int idx = PlayerPrefs.GetInt(PREF_INDEX, 0);
            MusicPlayer.Instance.PlayIndex(idx);
        }
    }

    // 主音量滑块变化
    private void OnMasterVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(PREF_MASTER, value);
        PlayerPrefs.Save();
    }

    // 音乐音量滑块变化
    private void OnMusicVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicVolume(value);
    }

    // 音乐开关按钮
    private void ToggleMusicOnOff()
    {
        _musicOn = !_musicOn;
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicOn(_musicOn);
        if (musicCheckmark != null)
            musicCheckmark.SetActive(_musicOn);
    }

    // 搜索输入变化：按关键字过滤列表按钮
    private void OnSearchTextChanged(string query)
    {
        if (_contentTransform == null) return;
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (Transform child in _contentTransform)
                child.gameObject.SetActive(true);
        }
        else
        {
            query = query.Trim().ToLowerInvariant();
            foreach (Transform child in _contentTransform)
            {
                var textComp = child.GetComponentInChildren<Text>();
                if (textComp != null && textComp.text.ToLowerInvariant().Contains(query))
                    child.gameObject.SetActive(true);
                else
                    child.gameObject.SetActive(false);
            }
        }
    }

    // 返回主菜单
    private void GoBack()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogError($"Scene not in Build Settings: {mainMenuSceneName}");
    }

    // 生成按名称排序的歌曲列表按钮
    private void BuildMusicListSortedByName()
    {
        if (MusicPlayer.Instance == null || _contentTransform == null || _templateTransform == null)
            return;
        ClearMusicButtons();

        var clips = MusicPlayer.Instance.songs;
        var infos = MusicSorter.BuildSongInfos(clips.ToArray());
        MusicSorter.BubbleSortByName(infos);
        CreateMusicButtonsFromInfos(infos);

        if (searchInput != null)
            searchInput.SetTextWithoutNotify("");
    }

    // 生成按时长排序的歌曲列表按钮
    private void BuildMusicListSortedByDuration()
    {
        if (MusicPlayer.Instance == null || _contentTransform == null || _templateTransform == null)
            return;
        ClearMusicButtons();

        var clips = MusicPlayer.Instance.songs;
        var infos = MusicSorter.BuildSongInfos(clips.ToArray());

        MusicSorter.ExchangeSortByDuration(infos);
        CreateMusicButtonsFromInfos(infos);

        if (searchInput != null)
            searchInput.SetTextWithoutNotify("");
    }

    // 生成按类型排序（类别优先，再按时长）的歌曲列表按钮
    private void BuildMusicListSortedByGenre()
    {
        if (MusicPlayer.Instance == null || _contentTransform == null || _templateTransform == null)
            return;
        ClearMusicButtons();

        var clips = MusicPlayer.Instance.songs;
        var infos = MusicSorter.BuildSongInfos(clips.ToArray());

        MusicSorter.SortByGenreThenDuration(infos);
        CreateMusicButtonsFromInfos(infos);

        if (searchInput != null)
            searchInput.SetTextWithoutNotify("");
    }

    // 清空已有歌曲按钮
    private void ClearMusicButtons()
    {
        if (_contentTransform == null) return;
        foreach (Transform child in _contentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    // 根据排序后的信息列表生成按钮
    private void CreateMusicButtonsFromInfos(List<MusicSorter.SongInfo> infos)
    {
        if (_templateTransform == null) return;
        for (int i = 0; i < infos.Count; i++)
        {
            int originalIndex = infos[i].OriginalIndex;
            string name = infos[i].Name;

            var btnObj = Instantiate(_templateTransform.gameObject, _contentTransform);
            btnObj.SetActive(true);
            btnObj.name = $"Song_{originalIndex}";

            var textComp = btnObj.GetComponentInChildren<Text>();
            if (textComp != null)
                textComp.text = name;

            var buttonComp = btnObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                int idx = originalIndex; // 捕获索引
                buttonComp.onClick.AddListener(() =>
                {
                    if (MusicPlayer.Instance != null)
                        MusicPlayer.Instance.PlayIndex(idx);
                });
            }
        }
    }

    // 工具：在活动场景中按名称查找组件
    private static T FindByNameInScene<T>(string name) where T : Component
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        var all = Resources.FindObjectsOfTypeAll<Transform>();
        var activeScene = SceneManager.GetActiveScene();
        for (int i = 0; i < all.Length; i++)
        {
            var t = all[i];
            if (t == null || t.name != name)
                continue;
            var go = t.gameObject;
            if (!go.scene.IsValid() || go.scene != activeScene)
                continue;
            return go.GetComponent<T>();
        }
        return null;
    }
}
