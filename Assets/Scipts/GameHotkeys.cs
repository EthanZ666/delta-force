using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.InputSystem;


public static class GameHotkeys
{
    // ===== Scene Names（必须和 Build Settings 完全一致）=====
    private const string SCENE_SPLASH = "SplashScene";
    private const string SCENE_MAIN_MENU = "MainMenuScene";
    private const string SCENE_MAP_SELECT = "MapSelectScene";
    private const string SCENE_SETTINGS = "SettingsScene";

    // 游戏关卡
    private const string SCENE_GAME_1 = "SampleScene";
    private const string SCENE_GAME_2 = "URP2DSceneTemplate";

    // ===== PlayerPrefs Keys（与你 Volume.cs 一致）=====
    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC = "volume_music";
    private const string PREF_MUSIC_ON = "music_on";

    private static bool _initialized;
    private static bool _paused;

    // 给 Volume UI 用的刷新事件
    public static event Action SettingsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 创建唯一监听器
        if (GameObject.Find("__GameHotkeys") == null)
        {
            var go = new GameObject("__GameHotkeys");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<Listener>();
        }

        EnsureMusicPlayerExists();
        ApplyFromPrefs();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _paused = false;
        Time.timeScale = 1f;

        EnsureMusicPlayerExists();
        ApplyFromPrefs();
    }

    private class Listener : MonoBehaviour
    {
        private void Awake()
        {
            var all = UnityEngine.Object.FindObjectsByType<Listener>(FindObjectsSortMode.None);
            if (all.Length > 1)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            HandleGlobalVolumeKeys();

            string s = SceneManager.GetActiveScene().name;

            // ===== Splash：任意键进主菜单 =====
            if (s == SCENE_SPLASH)
            {
                if (Input.anyKeyDown)
                    TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // ===== MainMenu：⚠️ 不再用 Enter 切场景（避免和 UI 冲突）=====
            if (s == SCENE_MAIN_MENU)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    Quit();
                return;
            }

            // ===== 地图选择：Esc 返回主菜单 =====
            if (s == SCENE_MAP_SELECT)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // ===== 设置：Esc 返回主菜单 =====
            if (s == SCENE_SETTINGS)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // ===== 游戏关卡：Esc 暂停 / 继续；Backspace 回主菜单 =====
            if (IsGameScene(s))
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_paused) Resume();
                    else Pause();
                }

                if (_paused && Input.GetKeyDown(KeyCode.Backspace))
                {
                    Resume();
                    TryLoad(SCENE_MAIN_MENU);
                }
            }
        }

        private static bool IsGameScene(string sceneName)
        {
            return sceneName == SCENE_GAME_1 || sceneName == SCENE_GAME_2;
        }

        private static void Pause()
        {
            _paused = true;
            Time.timeScale = 0f;
            Debug.Log("[Pause] ON");
        }

        private static void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            Debug.Log("[Pause] OFF");
        }

        private static void HandleGlobalVolumeKeys()
        {
            float v = AudioListener.volume;
            bool changed = false;

            if (Keyboard.current.equalsKey.wasPressedThisFrame ||
    Keyboard.current.numpadPlusKey.wasPressedThisFrame)
{
    v += 0.05f;
    changed = true;
}

if (Keyboard.current.minusKey.wasPressedThisFrame ||
    Keyboard.current.numpadMinusKey.wasPressedThisFrame)
{
    v -= 0.05f;
    changed = true;
}


            if (!changed) return;

            v = Mathf.Clamp01(v);
            ApplyMaster(v);

            PlayerPrefs.SetFloat(PREF_MASTER, v);
            PlayerPrefs.Save();

            SettingsChanged?.Invoke();
            Debug.Log($"[Master Volume] {v:0.00}");
        }

        private static void TryLoad(string sceneName)
        {
            Time.timeScale = 1f;
            _paused = false;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[Scene] Cannot load '{sceneName}'. Add it to Build Settings.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        private static void Quit()
        {
            Debug.Log("[Quit]");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // ===== Apply helpers =====
    private static void ApplyFromPrefs()
    {
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
        float music = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
        bool musicOn = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;

        ApplyMaster(master);
        ApplyMusic(music);
        ApplyMusicOn(musicOn);
    }

    private static void ApplyMaster(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }

    private static void ApplyMusic(float v)
    {
        var src = GetMusicSource();
        if (src != null) src.volume = Mathf.Clamp01(v);
    }

    private static void ApplyMusicOn(bool on)
    {
        var src = GetMusicSource();
        if (src != null) src.mute = !on;
    }

    private static AudioSource GetMusicSource()
    {
        if (MusicPlayer.Instance == null) return null;
        return MusicPlayer.Instance.GetSource();
    }

    // ===== 确保 MusicPlayer 存在（不依赖 AutoBootstrap）=====
    private static void EnsureMusicPlayerExists()
    {
        if (MusicPlayer.Instance != null) return;

        var existing = UnityEngine.Object.FindAnyObjectByType<MusicPlayer>();
        if (existing != null) return;

        var go = new GameObject("__MusicPlayer");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;

        var src = go.AddComponent<AudioSource>();
        src.loop = true;

        go.AddComponent<MusicPlayer>();
    }
}
