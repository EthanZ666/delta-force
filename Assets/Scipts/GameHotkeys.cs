using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameHotkeys
{
    private const string SCENE_SPLASH = "SplashScene";
    private const string SCENE_MAIN_MENU = "MainMenuScene";
    private const string SCENE_GAME = "SampleScene";

    // 你现在没有 MapSelectScene，所以先不启用（后面有再改）
    // private const string SCENE_MAP_SELECT = "MapSelectScene";

    private static bool _paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 创建一个常驻监听器（不需要你挂任何东西）
        var go = new GameObject("__GameHotkeys");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<Listener>();

        // 读取上次保存的音量（全局）
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("MASTER_VOL", 0.8f));
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _paused = false;
        Time.timeScale = 1f;
    }

    private class Listener : MonoBehaviour
    {
        private void Update()
        {
            // 全局音量快捷键（任何场景都能用）
            HandleGlobalVolumeKeys();

            string s = SceneManager.GetActiveScene().name;

            // Splash：按任意键跳主菜单（可选）
            if (s == SCENE_SPLASH)
            {
                if (Input.anyKeyDown)
                    TryLoad(SCENE_MAIN_MENU);
                return;
            }

            // 主菜单：Enter 开始游戏，Q 退出
            if (s == SCENE_MAIN_MENU)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    TryLoad(SCENE_GAME);

                if (Input.GetKeyDown(KeyCode.Q))
                    Quit();

                return;
            }

            // 游戏关卡：Esc 暂停/继续；暂停时 Backspace 回主菜单
            if (s == SCENE_GAME)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_paused) Resume();
                    else Pause();
                }

                if (_paused && Input.GetKeyDown(KeyCode.Backspace))
                {
                    _paused = false;
                    Time.timeScale = 1f;
                    TryLoad(SCENE_MAIN_MENU);
                }
                return;
            }

            // 其它场景：你现在先不管（比如 URP2DSceneTemplate）
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

            // + / - 或 [ / ] 调音量（兼容键盘布局）
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
            AudioListener.volume = v;

            PlayerPrefs.SetFloat("MASTER_VOL", v);
            PlayerPrefs.Save();

            Debug.Log($"[Volume] {v:0.00}");
        }

        private static void TryLoad(string sceneName)
        {
            // 这个检查可以避免 “Scene couldn't be loaded because it has not been added to the build settings”
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[Scene] Cannot load '{sceneName}'. Add it to Build Settings (or use an Editor auto-adder).");
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
}
