using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
//     {
//         var uiEventSystem = new GameObject("EventSystem");
//         uiEventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
//         uiEventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
//     }

public class MainMenuOverlayUI : MonoBehaviour
{
    [Header("Resources paths (no extension)")]
    [SerializeField] private string backgroundPath = "Images/MainMenuScene";
    [SerializeField] private string startButtonPath = "Images/StartButton";
    [SerializeField] private string settingsButtonPath = "Images/SettingButton";

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "MapSelectScene";
    [SerializeField] private string settingsSceneName = "SettingsScene";

    // 你可以微调这些（对齐你背景上的位置）
    [Header("Layout (tweak these numbers)")]
    [SerializeField] private Vector2 startAnchoredPos = new Vector2(0, -70);
    [SerializeField] private Vector2 settingsAnchoredPos = new Vector2(0, -230);

    [SerializeField] private Vector2 buttonSize = new Vector2(520, 140);

    private void Start()
    {
        Sprite bg = Resources.Load<Sprite>(backgroundPath);
        if (bg == null)
        {
            Debug.LogError($"MainMenu background not found at Resources/{backgroundPath}.png");
            return;
        }

        Sprite startSprite = Resources.Load<Sprite>(startButtonPath);
        if (startSprite == null)
        {
            Debug.LogError($"StartButton sprite not found at Resources/{startButtonPath}.png");
            return;
        }

        Sprite settingsSprite = Resources.Load<Sprite>(settingsButtonPath);
        if (settingsSprite == null)
        {
            Debug.LogError($"SettingButton sprite not found at Resources/{settingsButtonPath}.png");
            return;
        }

        CreateUI(bg, startSprite, settingsSprite);
    }

    private void CreateUI(Sprite backgroundSprite, Sprite startSprite, Sprite settingsSprite)
    {
        // Canvas
        var canvasGO = new GameObject("MainMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.sprite = backgroundSprite;
        bgImage.preserveAspect = true;

        var bgRT = bgImage.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Start button (visible sprite + click)
        CreateSpriteButton(
            parent: canvasGO.transform,
            name: "StartButton",
            sprite: startSprite,
            anchoredPos: startAnchoredPos,
            size: buttonSize,
            onClick: () => LoadSceneSafe(startSceneName)
        );

        // Settings button (visible sprite + click)
        CreateSpriteButton(
            parent: canvasGO.transform,
            name: "SettingsButton",
            sprite: settingsSprite,
            anchoredPos: settingsAnchoredPos,
            size: buttonSize,
            onClick: () => LoadSceneSafe(settingsSceneName)
        );
    }

    private void CreateSpriteButton(Transform parent, string name, Sprite sprite, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        var btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        var rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
    }

    private void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Cannot load scene '{sceneName}'. Add it to Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
