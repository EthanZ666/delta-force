// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class AutoBootstrap : MonoBehaviour
// {
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//     private static void Init()
//     {
// //        AddIfMissing<RuntimeUIBootstrap>();
//         AddIfMissing<MusicPlayer>();
// //        AddIfMissing<GameHotkeys>();
// //        AddIfMissing<MenuSystem>();
//         AddIfMissing<MoneyManager>();
//         // 如果还有别的要挂的记得写放这里，等我file没注释了记得帮我去注释化谢谢
//     }

//     private static void AddIfMissing<T>() where T : Component
//     {
//         if (Object.FindObjectOfType<T>() != null) return;

//         var go = new GameObject("__" + typeof(T).Name);
//         Object.DontDestroyOnLoad(go);
//         go.hideFlags = HideFlags.HideAndDontSave;
//         go.AddComponent<T>();
//     }
// }
