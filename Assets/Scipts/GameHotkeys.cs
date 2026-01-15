// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System;

// public static class GameHotkeys
// {
//     // ===== Scenes (按你真实名字改) =====
//     private const string SCENE_SPLASH = "SplashScene";
//     private const string SCENE_MAIN_MENU = "MainMenuScene";
//     private const string SCENE_MAP_SELECT = "MapSelectScene";
//     private const string SCENE_SETTINGS = "SettingsScene";

//     // 关卡场景（你有更多就继续加）
//     private const string SCENE_GAME_1 = "SampleScene";
//     private const string SCENE_GAME_2 = "URP2DSceneTemplate";

//     // ===== PlayerPrefs Keys (与你 Volume.cs 一致) =====
//     private const string PREF_MASTER = "volume_master";
//     private const string PREF_MUSIC = "volume_music";
//     private const string PREF_MUSIC_ON = "music_on";

//     // ===== 防重复初始化（Bug4） =====
//     private static bool _initialized;
//     private static bool _paused;

//     // ===== 通知 Settings UI 刷新（Bug5） =====
//     public static event Action SettingsChanged;

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//     private static void Init()
//     {
//         if (_initialized) return;
//         _initialized = true;

//         // 防止重复绑定
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//         SceneManager.sceneLoaded += OnSceneLoaded;

//         // 防止重复创建监听器对象
//         if (GameObject.Find("__GameHotkeys") == null)
//         {
//             var go = new GameObject("__GameHotkeys");
//             UnityEngine.Object.DontDestroyOnLoad(go);
//             go.hideFlags = HideFlags.HideAndDontSave;
//             go.AddComponent<Listener>();
//         }

//         // Bug3：确保任何场景（包括 SettingsScene）都有 MusicPlayer
//         EnsureMusicPlayerExists();

//         // 启动时应用一次上次保存的设置
//         ApplyFromPrefs();
//     }

//     private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // Bug2：切场景必定恢复
//         _paused = false;
//         Time.timeScale = 1f;

//         EnsureMusicPlayerExists();
//         ApplyFromPrefs();
//     }

//     private class Listener : MonoBehaviour
//     {
//         private void Awake()
//         {
//             // Unity 6：FindObjectsOfType 已过时，用 FindObjectsByType
//             var all = UnityEngine.Object.FindObjectsByType<Listener>(FindObjectsSortMode.None);
//             if (all.Length > 1)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//         }

//         private void Update()
//         {
//             HandleGlobalVolumeKeys();

//             string s = SceneManager.GetActiveScene().name;

//             // Splash：按任意键跳主菜单（可选）
//             if (s == SCENE_SPLASH)
//             {
//                 if (Input.anyKeyDown) TryLoad(SCENE_MAIN_MENU);
//                 return;
//             }

//             // 主菜单：Enter 去地图选择；Q 退出（可选）
//             if (s == SCENE_MAIN_MENU)
//             {
//                 if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
//                     TryLoad(SCENE_MAP_SELECT);

//                 if (Input.GetKeyDown(KeyCode.Q))
//                     Quit();

//                 return;
//             }

//             // 地图选择：Esc 返回主菜单
//             if (s == SCENE_MAP_SELECT)
//             {
//                 if (Input.GetKeyDown(KeyCode.Escape))
//                     TryLoad(SCENE_MAIN_MENU);
//                 return;
//             }

//             // 设置：Esc 返回主菜单
//             if (s == SCENE_SETTINGS)
//             {
//                 if (Input.GetKeyDown(KeyCode.Escape))
//                     TryLoad(SCENE_MAIN_MENU);
//                 return;
//             }

//             // 游戏关卡：Esc 暂停/继续；暂停时 Backspace 回主菜单
//             if (IsGameScene(s))
//             {
//                 if (Input.GetKeyDown(KeyCode.Escape))
//                 {
//                     if (_paused) Resume();
//                     else Pause();
//                 }

//                 if (_paused && Input.GetKeyDown(KeyCode.Backspace))
//                 {
//                     Resume();
//                     TryLoad(SCENE_MAIN_MENU);
//                 }
//             }
//         }

//         private static bool IsGameScene(string sceneName)
//         {
//             return sceneName == SCENE_GAME_1 || sceneName == SCENE_GAME_2;
//         }

//         private static void Pause()
//         {
//             _paused = true;
//             Time.timeScale = 0f;
//             Debug.Log("[Pause] ON  (Esc=Resume, Backspace=MainMenu)");
//         }

//         private static void Resume()
//         {
//             _paused = false;
//             Time.timeScale = 1f;
//             Debug.Log("[Pause] OFF");
//         }

//         private static void HandleGlobalVolumeKeys()
//         {
//             float v = AudioListener.volume;
//             bool changed = false;

//             // + / - 或 [ / ] 调主音量（兼容不同键盘）
//             if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.RightBracket))
//             {
//                 v += 0.05f;
//                 changed = true;
//             }
//             if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.LeftBracket))
//             {
//                 v -= 0.05f;
//                 changed = true;
//             }

//             if (!changed) return;

//             v = Mathf.Clamp01(v);
//             ApplyMaster(v);

//             PlayerPrefs.SetFloat(PREF_MASTER, v);
//             PlayerPrefs.Save();

//             // Bug5：通知 Settings UI 刷新
//             SettingsChanged?.Invoke();

//             Debug.Log($"[Master Volume] {v:0.00}");
//         }

//         private static void TryLoad(string sceneName)
//         {
//             // Bug2：任何切场景前强制恢复
//             Time.timeScale = 1f;
//             _paused = false;

//             if (!Application.CanStreamedLevelBeLoaded(sceneName))
//             {
//                 Debug.LogError($"[Scene] Cannot load '{sceneName}'. Add it to Build Settings.");
//                 return;
//             }

//             SceneManager.LoadScene(sceneName);
//         }

//         private static void Quit()
//         {
//             Debug.Log("[Quit]");
//             Application.Quit();
// #if UNITY_EDITOR
//             UnityEditor.EditorApplication.isPlaying = false;
// #endif
//         }
//     }

//     // ======= Apply helpers =======
//     private static void ApplyFromPrefs()
//     {
//         float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
//         float music = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
//         bool musicOn = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;

//         ApplyMaster(master);
//         ApplyMusic(music);
//         ApplyMusicOn(musicOn);
//     }

//     private static void ApplyMaster(float v)
//     {
//         AudioListener.volume = Mathf.Clamp01(v);
//     }

//     private static void ApplyMusic(float v)
//     {
//         var src = GetMusicSource();
//         if (src != null) src.volume = Mathf.Clamp01(v);
//     }

//     private static void ApplyMusicOn(bool on)
//     {
//         var src = GetMusicSource();
//         if (src != null) src.mute = !on;
//     }

//     private static AudioSource GetMusicSource()
//     {
//         if (MusicPlayer.Instance == null) return null;
//         return MusicPlayer.Instance.GetSource();
//     }

//     // Bug3：自动补齐 MusicPlayer（Unity 6：避免使用 FindObjectOfType）
//     private static void EnsureMusicPlayerExists()
//     {
//         if (MusicPlayer.Instance != null) return;

//         // Unity 6 推荐：FindAnyObjectByType
//         var existing = UnityEngine.Object.FindAnyObjectByType<MusicPlayer>();
//         if (existing != null) return;

//         var go = new GameObject("__MusicPlayer");
//         UnityEngine.Object.DontDestroyOnLoad(go);
//         go.hideFlags = HideFlags.HideAndDontSave;

//         var src = go.AddComponent<AudioSource>();
//         src.loop = true;

//         go.AddComponent<MusicPlayer>();
//     }
// }
