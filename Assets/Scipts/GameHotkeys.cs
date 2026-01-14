using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameHotkeys
{
    // 你项目里的场景名（大小写必须和 Scene 文件名一致）
    private const string SCENE_SPLASH = "SplashScene";
    private const string SCENE_MAIN_MENU = "MainMenuScene";
    private const string SCENE_MAP_SELECT = "MapSelectScene";
    private const string SCENE_SETTINGS = "SettingsScene";

    // 你游戏关卡场景（如果你有多个关卡，可以按需加 OR 判断）
    private const string SCENE_GAME_1 = "SampleScene";
    private const string SCENE_GAME_2 = "URP2DSceneTemplate";

    // PlayerPrefs keys：和 Volume.cs 完全一致
    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC = "volume_music";
    private const string PREF_MUSIC_ON = "music_on";

    private static bool _paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        // 防止重复创建监听器
        if (GameObject.Find("__GameHotkeys") != null)
        {
            // 仍然确保 sceneLoaded 绑定一次
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        var go = new GameObject("__GameHotkeys");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<Listener>();

        // 启动时应用上次保存的设置
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
        float music = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
        bool musicOn = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;

        ApplyMaster(master);
        ApplyMusic(music);
        ApplyMusicOn(musicOn);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _paused = false;
        Time.timeScale = 1f;

        // 切场景时再应用一次（防止 AudioListener / MusicPlayer 刚出现）
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
        float music = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
        bool musicOn = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;

        ApplyMaster(master);
        ApplyMusic(music);
        ApplyMusicOn(musicOn);
    }

    private class Listener : MonoBehaviour
    {
        private void Update()
        {
            HandleGlobalVolumeKeys();

            string s = SceneManager.GetActiveScene().name;

            // Splash：按任意键跳主菜单（可选）
            if (s == SCENE_SPLASH)
            {
                if (Input.anyKeyDown) TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // 主菜单：Enter 开始（去地图选择）；Q 退出（可选）
            if (s == SCENE_MAIN_MENU)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    TryLoad(SCENE_MAP_SELECT);

                if (Input.GetKeyDown(KeyCode.Q))
                    Quit();

                return;
            }

            // 地图选择：Esc 返回主菜单
            if (s == SCENE_MAP_SELECT)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // 设置：Esc 返回主菜单
            if (s == SCENE_SETTINGS)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // 游戏关卡：Esc 暂停/继续；暂停时 Backspace 回主菜单
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
                return;
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
            Debug.Log("[Pause] ON  (Esc=Resume, Backspace=MainMenu)");
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

            // + / - 或 [ / ] 调主音量（兼容键盘布局）
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.RightBracket))
            {
                v += 0.05f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.LeftBracket))
            {
                v -= 0.05f;
                changed = true;
            }

            if (!changed) return;

            v = Mathf.Clamp01(v);
            ApplyMaster(v);

            PlayerPrefs.SetFloat(PREF_MASTER, v);
            PlayerPrefs.Save();

            Debug.Log($"[Master Volume] {v:0.00}");
        }

        private static void TryLoad(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[Scene] Cannot load '{sceneName}'. Add it to Build Settings.");
                return;
            }

            Time.timeScale = 1f;
            _paused = false;
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

    // ---- Apply helpers（和 Volume.cs 同一套逻辑） ----
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
}
