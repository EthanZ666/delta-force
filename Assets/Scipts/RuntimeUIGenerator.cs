// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;

// public static class RuntimeUIGenerator
// {
//     private const string ROOT_NAME = "__RuntimeUIRoot";

//     // UI 组件引用
//     private static Text _hudText;
//     private static Text _centerText;
//     private static GameObject _resultPanel;
//     private static Text _resultText;

//     public static void EnsureUI()
//     {
//         if (GameObject.Find(ROOT_NAME) != null) return;

//         EnsureEventSystem();

//         // Root
//         var root = new GameObject(ROOT_NAME);
//         root.hideFlags = HideFlags.HideAndDontSave;

//         // Canvas
//         var canvasGO = new GameObject("Canvas");
//         canvasGO.transform.SetParent(root.transform);
//         var canvas = canvasGO.AddComponent<Canvas>();
//         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//         canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//         canvasGO.AddComponent<GraphicRaycaster>();

//         // HUD（左上角）
//         _hudText = CreateText(canvasGO.transform, "HUDText", new Vector2(10, -10), TextAnchor.UpperLeft, 18);
//         _hudText.text = "HUD: loading...";

//         // Center（屏幕中间提示：Ready/Wave/BOSS）
//         _centerText = CreateText(canvasGO.transform, "CenterText", new Vector2(0, 0), TextAnchor.MiddleCenter, 34);
//         _centerText.text = "";
//         _centerText.enabled = false;

//         // Result Panel（Win/Lose）
//         _resultPanel = new GameObject("ResultPanel");
//         _resultPanel.transform.SetParent(canvasGO.transform);
//         var img = _resultPanel.AddComponent<Image>();
//         img.color = new Color(0, 0, 0, 0.65f);

//         var rt = _resultPanel.GetComponent<RectTransform>();
//         rt.anchorMin = Vector2.zero;
//         rt.anchorMax = Vector2.one;
//         rt.offsetMin = Vector2.zero;
//         rt.offsetMax = Vector2.zero;

//         _resultText = CreateText(_resultPanel.transform, "ResultText", new Vector2(0, 0), TextAnchor.MiddleCenter, 44);
//         _resultText.text = "";
//         _resultPanel.SetActive(false);

//         // 生成一个 updater（负责每帧刷新 HUD）
//         var updaterGO = new GameObject("HUDUpdater");
//         updaterGO.transform.SetParent(root.transform);
//         updaterGO.hideFlags = HideFlags.HideAndDontSave;
//         updaterGO.AddComponent<RuntimeUIUpdater>();
//     }

//     public static void ShowCenter(string msg, float seconds = 1.2f)
//     {
//         if (_centerText == null) return;
//         _centerText.enabled = true;
//         _centerText.text = msg;
//         RuntimeUIUpdater.RunAfter(seconds, () =>
//         {
//             if (_centerText != null)
//             {
//                 _centerText.text = "";
//                 _centerText.enabled = false;
//             }
//         });
//     }

//     public static void ShowResult(string msg)
//     {
//         if (_resultPanel == null || _resultText == null) return;
//         _resultText.text = msg + "\n\nPress Backspace to return";
//         _resultPanel.SetActive(true);
//     }

//     public static void HideResult()
//     {
//         if (_resultPanel != null) _resultPanel.SetActive(false);
//     }

//     public static void SetHUDText(string msg)
//     {
//         if (_hudText != null) _hudText.text = msg;
//     }

//     private static Text CreateText(Transform parent, string name, Vector2 anchoredPos, TextAnchor anchor, int fontSize)
//     {
//         var go = new GameObject(name);
//         go.transform.SetParent(parent);
//         var text = go.AddComponent<Text>();
//         text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//         text.fontSize = fontSize;
//         text.alignment = anchor;
//         text.color = Color.white;

//         var rt = go.GetComponent<RectTransform>();
//         rt.sizeDelta = new Vector2(900, 300);

//         // anchor 位置
//         if (anchor == TextAnchor.UpperLeft)
//         {
//             rt.anchorMin = new Vector2(0, 1);
//             rt.anchorMax = new Vector2(0, 1);
//             rt.pivot = new Vector2(0, 1);
//         }
//         else
//         {
//             rt.anchorMin = new Vector2(0.5f, 0.5f);
//             rt.anchorMax = new Vector2(0.5f, 0.5f);
//             rt.pivot = new Vector2(0.5f, 0.5f);
//         }

//         rt.anchoredPosition = anchoredPos;
//         return text;
//     }

//     private static void EnsureEventSystem()
//     {
//         if (Object.FindObjectOfType<EventSystem>() != null) return;

//         var es = new GameObject("EventSystem");
//         es.hideFlags = HideFlags.HideAndDontSave;
//         es.AddComponent<EventSystem>();
//         es.AddComponent<StandaloneInputModule>();
//     }
// }
