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

        // Load sprites from Resources
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
        // If another scene/manager already made one, do nothing.
        if (EventSystem.current != null) return;

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        // New Input System: required for UI clicks when InputManager is disabled
        esGO.AddComponent<InputSystemUIInputModule>();
#else
        // Old Input System fallback
        esGO.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateUI(Sprite backgroundSprite, Sprite startSprite, Sprite settingsSprite)
    {
        // If a previous run left a canvas (rare), clean it.
        var existing = GameObject.Find("MainMenuCanvas");
        if (existing != null) Destroy(existing);

        // Canvas
        var canvasGO = new GameObject("MainMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Background (IMPORTANT: do not block clicks)
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.sprite = backgroundSprite;
        bgImage.preserveAspect = true;
        bgImage.raycastTarget = false; // ✅ prevents eating clicks

        var bgRT = bgImage.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Start Button
        CreateSpriteButton(
            parent: canvasGO.transform,
            name: "StartButton",
            sprite: startSprite,
            anchoredPos: startAnchoredPos,
            size: buttonSize,
            onClick: () =>
            {
                Debug.Log("START CLICKED");
                LoadSceneSafe(startSceneName);
            }
        );

        // Settings Button
        CreateSpriteButton(
            parent: canvasGO.transform,
            name: "SettingsButton",
            sprite: settingsSprite,
            anchoredPos: settingsAnchoredPos,
            size: buttonSize,
            onClick: () =>
            {
                Debug.Log("SETTINGS CLICKED");
                LoadSceneSafe(settingsSceneName);
            }
        );
    }

    private void CreateSpriteButton(Transform parent, string name, Sprite sprite, Vector2 anchoredPos, Vector2 size, System.Action onClick)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        // RectTransform
        var rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Image
        var img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = true; // ✅ make sure it receives clicks

        // Button
        var btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        btn.onClick.AddListener(() =>
        {
            try { onClick?.Invoke(); }
            catch (System.Exception e) { Debug.LogError(e); }
        });
    }

    private void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        // This checks if the scene is in Build Settings
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Cannot load scene '{sceneName}'. Add it to Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
