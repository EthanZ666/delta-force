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

        // ✅✅✅ Map Select：方案A => 这里不处理 Esc（交给 MapSelector 自己处理 PausePanel）
        if (s == SCENE_MAP_SELECT)
        {
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

        if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
        {
            v += 0.05f;
            changed = true;
        }

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

    // ✅ NEW: 在当前场景里找到（包含 inactive）并 toggle 指定面板
    private static void TogglePanelInActiveScene(string panelName)
    {
        var activeScene = SceneManager.GetActiveScene();

        var all = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < all.Length; i++)
        {
            var t = all[i];
            if (t == null) continue;
            if (t.name != panelName) continue;

            var go = t.gameObject;

            // 排除 prefab/资源，只要场景里真实物体
            if (!go.scene.IsValid()) continue;

            // 只切当前激活场景里的
            if (go.scene != activeScene) continue;

            go.SetActive(!go.activeSelf);
            return;
        }

        Debug.LogWarning($"[GameHotkeys] '{panelName}' not found in active scene '{activeScene.name}'.");
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
