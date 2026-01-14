using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuOverlayUI : MonoBehaviour
{
    [Header("Resources path: Resources/Images/MainMenuScene.png")]
    [SerializeField] private string backgroundPath = "Images/MainMenuScene";

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "MapSelectScene";     // 你开始按钮要去的场景
    [SerializeField] private string settingsSceneName = "SettingsScene";   // 你设置按钮要去的场景(没有就先不填)

    private void Start()
    {
        Sprite backgroundSprite = Resources.Load<Sprite>(backgroundPath);
        if (backgroundSprite == null)
        {
            Debug.LogError($"MainMenu image not found at Resources/{backgroundPath}.png");
            return;
        }

        CreateUI(backgroundSprite);
    }

    void CreateUI(Sprite backgroundSprite)
    {
        // Canvas
        var canvasGO = new GameObject("MainMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 背景图
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.sprite = backgroundSprite;
        bgImage.rectTransform.anchorMin = Vector2.zero;
        bgImage.rectTransform.anchorMax = Vector2.one;
        bgImage.rectTransform.offsetMin = Vector2.zero;
        bgImage.rectTransform.offsetMax = Vector2.zero;
        bgImage.preserveAspect = true;

        // ✅ START 按钮（你需要把位置对准你图上的 START 框）
        var startBtn = CreateTransparentButton(
            name: "StartButton",
            anchoredPos: new Vector2(0, -70),   // 这两个数你自己微调
            size: new Vector2(420, 120),
            onClick: () => LoadSceneSafe(startSceneName)
        );
        startBtn.transform.SetParent(canvasGO.transform, false);

        // ✅ SETTINGS 按钮（对准 SETTINGS 框）
        var settingsBtn = CreateTransparentButton(
            name: "SettingsButton",
            anchoredPos: new Vector2(0, -230),  // 这两个数你自己微调
            size: new Vector2(420, 120),
            onClick: () => OnClickSettings()
        );
        settingsBtn.transform.SetParent(canvasGO.transform, false);
    }

    void OnClickSettings()
    {
        // 1) 如果你有独立 SettingsScene，就切场景
        if (!string.IsNullOrEmpty(settingsSceneName))
        {
            LoadSceneSafe(settingsSceneName);
            return;
        }

        // 2) 还没做 SettingsScene 的话，先打个 log，等你以后再接设置面板
        Debug.Log("Settings clicked (no SettingsSceneName set).");
    }

    Button CreateTransparentButton(string name, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        var img = btnObj.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0); // 完全透明

        var btn = btnObj.AddComponent<Button>();

        var rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        btn.onClick.AddListener(onClick);
        return btn;
    }

    void LoadSceneSafe(string sceneName)
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
