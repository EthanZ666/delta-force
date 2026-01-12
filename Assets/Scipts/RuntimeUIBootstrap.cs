// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class RuntimeUIBootstrap : MonoBehaviour
// {
//     private static RuntimeUIBootstrap _instance;

//     private void Awake()
//     {
//         if (_instance != null) { Destroy(gameObject); return; }
//         _instance = this;
//         DontDestroyOnLoad(gameObject);

//         SceneManager.sceneLoaded += OnSceneLoaded;
//     }

//     private void OnDestroy()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//     private static void AutoCreate()
//     {
//         if (FindObjectOfType<RuntimeUIBootstrap>() != null) return;

//         var go = new GameObject("__RuntimeUIBootstrap");
//         go.hideFlags = HideFlags.HideAndDontSave;
//         go.AddComponent<RuntimeUIBootstrap>();
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // 每个新场景都确保 UI 存在（但不会重复创建）
//         RuntimeUIGenerator.EnsureUI();
//     }
// }
