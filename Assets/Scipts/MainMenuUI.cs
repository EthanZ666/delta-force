// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// //Angela等你弄好MainMenu之后记得来改code就是那个CreateMainMenuUI和CreateButton
// public class MainMenuUI : MonoBehaviour
// {
//     private void Start()
//     {
//         CreateMainMenuUI();
//     }

//     void CreateMainMenuUI()
//     {
//         var canvasGO = new GameObject("MainMenuCanvas");
//         canvasGO.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
//         canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//         canvasGO.AddComponent<GraphicRaycaster>();

//         // Start Game Button
//         var startButton = CreateButton("Start Game", new Vector2(0, 60), () =>
//         {
//             SceneManager.LoadScene("MapSelectScene");
//         });
//         startButton.transform.SetParent(canvasGO.transform, false);

//         // Settings Button
//         var settingsButton = CreateButton("Settings", new Vector2(0, -20), () =>
//         {
//             SceneManager.LoadScene("SettingScene"); // 用于调音量的场景名
//         });
//         settingsButton.transform.SetParent(canvasGO.transform, false);
//     }

//     Button CreateButton(string text, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
//     {
//         GameObject btnObj = new GameObject(text + "Button");
//         var btn = btnObj.AddComponent<Button>();
//         var img = btnObj.AddComponent<Image>();
//         img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

//         // RectTransform
//         var rt = btnObj.GetComponent<RectTransform>();
//         rt.sizeDelta = new Vector2(200, 50);
//         rt.anchorMin = new Vector2(0.5f, 0.5f);
//         rt.anchorMax = new Vector2(0.5f, 0.5f);
//         rt.anchoredPosition = anchoredPos;

//         // Text
//         GameObject textObj = new GameObject("Text");
//         textObj.transform.SetParent(btnObj.transform, false);
//         var txt = textObj.AddComponent<Text>();
//         txt.text = text;
//         txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//         txt.alignment = TextAnchor.MiddleCenter;
//         txt.color = Color.white;

//         var txtRT = textObj.GetComponent<RectTransform>();
//         txtRT.anchorMin = Vector2.zero;
//         txtRT.anchorMax = Vector2.one;
//         txtRT.offsetMin = Vector2.zero;
//         txtRT.offsetMax = Vector2.zero;

//         btn.onClick.AddListener(onClick);
//         return btn;
//     }
// }