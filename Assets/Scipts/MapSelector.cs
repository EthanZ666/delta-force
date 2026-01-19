using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class MapSelector : MonoBehaviour
{
    public enum ModeType
    {
        MapSelectUI,
        ApplyInGameplay
    }

    [Header("Mode")]
    public ModeType mode = ModeType.MapSelectUI;

    [Header("Scene Names")]
    public string mapSelectSceneName = "MapSelectScene";
    public string gameplaySceneName = "SampleScene";

    [Header("Map IDs (order matters)")]
    public List<string> mapIds = new List<string> { "Daba", "Zongcai" };

    [Header("Preview Sprites (Resources paths, no extension)")]
    public List<string> mapPreviewPaths = new List<string>
    {
        "Images/Daba",
        "Images/Zongcai"
    };

    [Header("PlayerPrefs")]
    public string selectedMapKey = "selected_map_id";
    public string fallbackMapId = "Daba";

    [Header("Gameplay Map Roots (ApplyInGameplay only)")]
    public List<GameObject> mapRoots = new List<GameObject>();

    // ============================
    // UI layout (MapSelectUI)
    // ============================
    [Header("UI Target Area")]
    [Tooltip("Drag ScenarioSlots here if you want. If empty, it will auto-find by name.")]
    public RectTransform targetArea;

    [Tooltip("Auto-find object by this name (recommended: ScenarioSlots under Canvas).")]
    public string autoFindTargetAreaName = "ScenarioSlots";

    [Header("Button Layout (Map previews)")]
    [Tooltip("Size of each map preview button (W,H). If maps look too tall, reduce H.")]
    public Vector2 buttonSize = new Vector2(520, 220); // 默认把高度变矮（原来320太高）

    [Tooltip("Spacing between two map buttons.")]
    public float spacing = 80f;

    [Header("ScenarioSlots Area Control (force by code)")]
    [Tooltip("Force ScenarioSlots to use center anchor/pivot and these size/position. This prevents teammates from messing it up.")]
    public bool forceTargetAreaRect = true;

    [Tooltip("ScenarioSlots Rect size (the black area space). If 0, it auto-calculates from buttons.")]
    public Vector2 targetAreaSize = Vector2.zero;

    [Tooltip("ScenarioSlots anchoredPosition. Use this to move the two maps up/down/left/right into the black box.")]
    public Vector2 targetAreaAnchoredPos = new Vector2(0f, 80f); // 往上提一点，接近黑框中间

    [Header("Optional: Crop (Mask)")]
    [Tooltip("If true, add a Mask so images never overflow the button rect.")]
    public bool addMaskToButtons = true;

    private GameObject _generatedCanvas;

    // ============================
    // Unity lifecycle
    // ============================
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (mode == ModeType.MapSelectUI)
        {
            if (!IsCurrentScene(mapSelectSceneName))
            {
                Destroy(gameObject);
                return;
            }
        }

        if (mode == ModeType.ApplyInGameplay)
        {
            ApplySelectedMapToRoots();
        }
    }

    private void Start()
    {
        if (mode != ModeType.MapSelectUI) return;

        EnsureEventSystem();

        if (targetArea == null && !string.IsNullOrWhiteSpace(autoFindTargetAreaName))
        {
            var go = GameObject.Find(autoFindTargetAreaName);
            if (go != null) targetArea = go.GetComponent<RectTransform>();
        }

        BuildUI();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (mode != ModeType.MapSelectUI) return;

        if (!IsCurrentScene(mapSelectSceneName))
        {
            DestroyGeneratedUI();
            Destroy(gameObject);
        }
        else
        {
            // 回到 MapSelectScene 时，确保 UI 仍然正确
            BuildUI();
        }
    }

    // ============================
    // Core UI logic
    // ============================
    private void BuildUI()
    {
        DestroyGeneratedUI();

        ValidateLists();

        if (targetArea != null)
        {
            BuildButtonsUnderTargetArea(targetArea);
            return;
        }

        // fallback canvas（不推荐）
        _generatedCanvas = new GameObject("MapSelectCanvas");
        var canvas = _generatedCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = _generatedCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _generatedCanvas.AddComponent<GraphicRaycaster>();

        BuildButtonsUnderTargetArea(_generatedCanvas.GetComponent<RectTransform>());
    }

    private void BuildButtonsUnderTargetArea(RectTransform parent)
    {
        // 1️⃣ 清理旧按钮
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var c = parent.GetChild(i);
            if (c != null && c.name.StartsWith("MapBtn_", StringComparison.OrdinalIgnoreCase))
            {
                Destroy(c.gameObject);
            }
        }

        // 2️⃣ 确保 HorizontalLayoutGroup，并且强制“不会被队友改乱”
        var hlg = parent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();

        // 强制写回（避免你说的自动变成 420）
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = spacing;

        // 重点：这些会导致“你在 Inspector 调不了 / 自动乱跳”，所以我们明确固定
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // padding 默认为 0（你需要也可以调）
        hlg.padding = new RectOffset(0, 0, 0, 0);

        // 3️⃣ 强制 ScenarioSlots 的 Rect（让它进黑框里）
        if (forceTargetAreaRect)
        {
            parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
            parent.pivot = new Vector2(0.5f, 0.5f);

            // 如果 targetAreaSize = 0，就自动根据按钮数量计算
            Vector2 finalSize = targetAreaSize;
            if (finalSize == Vector2.zero)
            {
                int count = Mathf.Max(1, mapIds.Count);
                float width = count * buttonSize.x + (count - 1) * spacing;
                float height = buttonSize.y;
                finalSize = new Vector2(width, height);
            }

            parent.sizeDelta = finalSize;
            parent.anchoredPosition = targetAreaAnchoredPos;
        }

        // 4️⃣ 生成按钮（每个按钮固定尺寸 + 可选Mask）
        for (int i = 0; i < mapIds.Count; i++)
        {
            string id = mapIds[i];
            string path = i < mapPreviewPaths.Count ? mapPreviewPaths[i] : "";

            var btnGO = new GameObject($"MapBtn_{id}");
            btnGO.transform.SetParent(parent, false);

            var rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = buttonSize;

            // LayoutElement：让 LayoutGroup 绝对按这个尺寸排版
            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredWidth = buttonSize.x;
            le.preferredHeight = buttonSize.y;
            le.minWidth = buttonSize.x;
            le.minHeight = buttonSize.y;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            // 可选：加 Mask，防止图片溢出按钮框（你说“高太长/挡住”这种最稳）
            RectTransform imageRect = rect;
            if (addMaskToButtons)
            {
                var maskImg = btnGO.AddComponent<Image>();
                maskImg.color = new Color(1, 1, 1, 1);
                var mask = btnGO.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                // 真正显示图片的子物体
                var imgChild = new GameObject("Preview");
                imgChild.transform.SetParent(btnGO.transform, false);
                imageRect = imgChild.AddComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;

                var img = imgChild.AddComponent<Image>();
                ApplySprite(img, id, path);

                // Button 放在父物体上
                var btn = btnGO.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.targetGraphic = maskImg;
                btn.onClick.AddListener(() => SelectMapAndLoad(id));
            }
            else
            {
                // 不使用 Mask：按钮本体显示图片
                var img = btnGO.AddComponent<Image>();
                ApplySprite(img, id, path);

                var btn = btnGO.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => SelectMapAndLoad(id));
            }
        }
    }

    private void ApplySprite(Image img, string id, string path)
    {
        img.raycastTarget = true;
        img.type = Image.Type.Simple;

        // PreserveAspect = true：会“按比例缩进按钮框内”
        // 你想“更矮一点”就调 buttonSize.y
        img.preserveAspect = true;

        Sprite sprite = null;
        if (!string.IsNullOrWhiteSpace(path))
            sprite = Resources.Load<Sprite>(path);

        if (sprite != null)
        {
            img.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"[MapSelector] Missing sprite for '{id}': Resources/{path}.png");
        }
    }

    // ============================
    // Map select / apply
    // ============================
    private void SelectMapAndLoad(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            Debug.LogError("[MapSelector] mapId is empty.");
            return;
        }

        PlayerPrefs.SetString(selectedMapKey, mapId);
        PlayerPrefs.Save();

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError($"[MapSelector] Scene not in Build Settings: {gameplaySceneName}");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void ApplySelectedMapToRoots()
    {
        foreach (var root in mapRoots)
            if (root != null)
                root.SetActive(false);

        string selected = PlayerPrefs.GetString(selectedMapKey, fallbackMapId);
        int index = mapIds.IndexOf(selected);
        if (index < 0 || index >= mapRoots.Count)
            index = mapIds.IndexOf(fallbackMapId);

        if (index >= 0 && index < mapRoots.Count && mapRoots[index] != null)
            mapRoots[index].SetActive(true);
    }

    // ============================
    // Utils
    // ============================
    private bool IsCurrentScene(string name)
    {
        return SceneManager.GetActiveScene().name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    private void DestroyGeneratedUI()
    {
        if (_generatedCanvas != null)
        {
            Destroy(_generatedCanvas);
            _generatedCanvas = null;
        }
    }

    private void ValidateLists()
    {
        if (mapIds == null || mapIds.Count == 0)
        {
            Debug.LogError("[MapSelector] mapIds is empty.");
            return;
        }

        if (mapPreviewPaths == null) mapPreviewPaths = new List<string>();

        // 如果预览路径少于 mapIds，提示但不崩
        if (mapPreviewPaths.Count < mapIds.Count)
        {
            Debug.LogWarning($"[MapSelector] mapPreviewPaths ({mapPreviewPaths.Count}) < mapIds ({mapIds.Count}). Some maps may have no preview sprite.");
        }
    }
}
