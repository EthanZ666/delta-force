using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.InputSystem;

public static class GameHotkeys
{
    // ===== Scene Names =====
    private const string SCENE_SPLASH = "SplashScene";
    private const string SCENE_MAIN_MENU = "MainMenuScene";
    private const string SCENE_MAP_SELECT = "MapSelectScene";
    private const string SCENE_SETTINGS = "SettingsScene";

    // ===== PlayerPrefs Keys =====
    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC = "volume_music";
    private const string PREF_MUSIC_ON = "music_on";

    private static bool _paused;

    public static event Action SettingsChanged;

    public static void Tick()
    {
        var kb = Keyboard.current;
        if (kb == null) return; // 没键盘就不处理

        HandleGlobalVolumeKeys(kb);

        string s = SceneManager.GetActiveScene().name;

        // ===== Splash：任意键进主菜单 =====
        if (s == SCENE_SPLASH)
        {
            // New Input System 的 anyKey
            if (kb.anyKey.wasPressedThisFrame)
                TryLoad(SCENE_MAIN_MENU);
            return;
        }

        // ===== Main Menu：Q 退出 =====
        if (s == SCENE_MAIN_MENU)
        {
            if (kb.qKey.wasPressedThisFrame)
                Quit();
            return;
        }

        // ===== Map Select：Esc 返回主菜单 =====
        if (s == SCENE_MAP_SELECT)
        {
            if (kb.escapeKey.wasPressedThisFrame)
                TryLoad(SCENE_MAIN_MENU);
            return;
        }

        // ===== Settings：Esc 返回主菜单 =====
        if (s == SCENE_SETTINGS)
        {
            if (kb.escapeKey.wasPressedThisFrame)
                TryLoad(SCENE_MAIN_MENU);
            return;
        }

        // ===== Gameplay（不是菜单的都算）=====
        if (IsGameplayScene(s))
        {
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (_paused) Resume();
                else Pause();
            }

            if (_paused && kb.backspaceKey.wasPressedThisFrame)
            {
                Resume();
                TryLoad(SCENE_MAIN_MENU);
            }
        }
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName != SCENE_MAIN_MENU
            && sceneName != SCENE_MAP_SELECT
            && sceneName != SCENE_SETTINGS
            && sceneName != SCENE_SPLASH;
    }

    private static void Pause()
    {
        _paused = true;
        Time.timeScale = 0f;

        var ui = UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>();
        if (ui != null) ui.Show();

        Debug.Log("[Pause] ON");
    }

    private static void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;

        var ui = UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>();
        if (ui != null) ui.Hide();

        Debug.Log("[Pause] OFF");
    }

    private static void HandleGlobalVolumeKeys(Keyboard kb)
    {
        float v = AudioListener.volume;
        bool changed = false;

        // +（=） 或 小键盘 +
        if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
        {
            v += 0.05f;
            changed = true;
        }

        // - 或 小键盘 -
        if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame)
        {
            v -= 0.05f;
            changed = true;
        }

        if (!changed) return;

        v = Mathf.Clamp01(v);
        AudioListener.volume = v;

        PlayerPrefs.SetFloat(PREF_MASTER, v);
        PlayerPrefs.Save();

        SettingsChanged?.Invoke();
        Debug.Log($"[Master Volume] {v:0.00}");
    }

    private static void TryLoad(string sceneName)
    {
        Time.timeScale = 1f;
        _paused = false;

        var ui = UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>();
        if (ui != null) ui.Hide();

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
