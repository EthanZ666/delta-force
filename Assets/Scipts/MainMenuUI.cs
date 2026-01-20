using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class MainMenuOverlayUI : MonoBehaviour
{
    [Header("Resources paths (no extension)")]
    [SerializeField] private string backgroundPath = "Images/MainMenuScene";
    [SerializeField] private string startButtonPath = "Images/StartButton";
    [SerializeField] private string settingsButtonPath = "Images/SettingButton";

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "MapSelectScene";
    [SerializeField] private string settingsSceneName = "SettingsScene";

    [Header("Layout (tweak these numbers)")]
    [SerializeField] private Vector2 startAnchoredPos = new Vector2(0, -70);
    [SerializeField] private Vector2 settingsAnchoredPos = new Vector2(0, -230);
    [SerializeField] private Vector2 buttonSize = new Vector2(520, 140);

    private void Start()
    {
        EnsureEventSystem();

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

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        esGO.AddComponent<InputSystemUIInputModule>();
#else
        esGO.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateUI(Sprite backgroundSprite, Sprite startSprite, Sprite settingsSprite)
    {
        var existing = GameObject.Find("MainMenuCanvas");
        if (existing != null) Destroy(existing);

        var canvasGO = new GameObject("MainMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ===== Background (FULL SCREEN, NO BLUE EDGE) =====
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);

        var bgImage = bgGO.AddComponent<Image>();
        bgImage.sprite = backgroundSprite;

        // ✅ 关键改动：铺满屏幕
        bgImage.preserveAspect = false;
        bgImage.raycastTarget = false;

        var bgRT = bgImage.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // ===== Start Button =====
        CreateSpriteButton(
            canvasGO.transform,
            "StartButton",
            startSprite,
            startAnchoredPos,
            buttonSize,
            () => LoadSceneSafe(startSceneName)
        );

        // ===== Settings Button =====
        CreateSpriteButton(
            canvasGO.transform,
            "SettingsButton",
            settingsSprite,
            settingsAnchoredPos,
            buttonSize,
            () => LoadSceneSafe(settingsSceneName)
        );
    }

    private void CreateSpriteButton(
        Transform parent,
        string name,
        Sprite sprite,
        Vector2 anchoredPos,
        Vector2 size,
        System.Action onClick)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = true;

        var btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    private void LoadSceneSafe(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Cannot load scene '{sceneName}'. Add it to Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
