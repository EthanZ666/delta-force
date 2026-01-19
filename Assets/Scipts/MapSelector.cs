using TMPro;
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

    // ============================
    // NEW: Right-click History UI
    // ============================
    [Header("History (Right Click)")]
    [Tooltip("History strings in the same order as mapIds. Right-click a map to show.")]
    public List<string> mapHistories = new List<string>
    {
        "Daba: (write your history here)",
        "Zongcai: (write your history here)"
    };

    [Tooltip("A Panel GameObject name under Canvas to show history.")]
    public string historyPanelName = "HistoryPanel";

    [Tooltip("A Text GameObject name (Unity UI Text) under HistoryPanel.")]
    public string historyTextName = "HistoryText";

    private GameObject _historyPanel;
    private TMP_Text _historyText;

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

        SetupHistoryUI();
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
            SetupHistoryUI();
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

        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = spacing;

        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        hlg.padding = new RectOffset(0, 0, 0, 0);

        // 3️⃣ 强制 ScenarioSlots 的 Rect（让它进黑框里）
        if (forceTargetAreaRect)
        {
            parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
            parent.pivot = new Vector2(0.5f, 0.5f);

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

        // 4️⃣ 生成按钮
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

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredWidth = buttonSize.x;
            le.preferredHeight = buttonSize.y;
            le.minWidth = buttonSize.x;
            le.minHeight = buttonSize.y;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            if (addMaskToButtons)
            {
                var maskImg = btnGO.AddComponent<Image>();
                maskImg.color = new Color(1, 1, 1, 1);

                var mask = btnGO.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                var imgChild = new GameObject("Preview");
                imgChild.transform.SetParent(btnGO.transform, false);
                var imageRect = imgChild.AddComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;

                var img = imgChild.AddComponent<Image>();
                ApplySprite(img, id, path);

                var btn = btnGO.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.targetGraphic = maskImg;
                btn.onClick.AddListener(() => SelectMapAndLoad(id));

                AddRightClickHistory(btnGO, id, i);
            }
            else
            {
                var img = btnGO.AddComponent<Image>();
                ApplySprite(img, id, path);

                var btn = btnGO.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => SelectMapAndLoad(id));

                AddRightClickHistory(btnGO, id, i);
            }
        }
    }

    private void ApplySprite(Image img, string id, string path)
    {
        img.raycastTarget = true;
        img.type = Image.Type.Simple;
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
    // History (Right click)
    // ============================
    private void SetupHistoryUI()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogWarning("[MapSelector] No Canvas found in scene.");
            return;
        }

        Transform panelTf = canvas.transform.Find(historyPanelName);
        if (panelTf == null)
        {
            var all = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == historyPanelName)
                {
                    if (all[i].gameObject.scene.IsValid())
                    {
                        panelTf = all[i];
                        break;
                    }
                }
            }
        }

        if (panelTf == null)
        {
            Debug.LogWarning($"[MapSelector] '{historyPanelName}' not found. Create a Panel named '{historyPanelName}' under Canvas.");
            return;
        }

        _historyPanel = panelTf.gameObject;

        Transform textTf = _historyPanel.transform.Find(historyTextName);
        if (textTf != null)
            _historyText = textTf.GetComponent<TMP_Text>();

        if (_historyText == null)
            _historyText = _historyPanel.GetComponentInChildren<TMP_Text>(true);

        if (_historyText == null)
        {
            Debug.LogWarning($"[MapSelector] No TMP_Text found. Create a 'TextMeshPro - Text (UI)' named '{historyTextName}' under '{historyPanelName}'.");
            return;
        }

        // ============================
        // ✅ NEW: 不拦截点击 + 不整屏变灰
        // ============================
        // 1) 让 Panel 及其所有子UI都不吃点击（这样地图还能点）
        var graphics = _historyPanel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = false;
        }

        // 2) 如果 HistoryPanel 自己有 Image（你整屏变灰就是它），直接设透明（只留文字）
        var panelImg = _historyPanel.GetComponent<Image>();
        if (panelImg != null)
        {
            panelImg.color = new Color(panelImg.color.r, panelImg.color.g, panelImg.color.b, 0f);
        }

        _historyPanel.SetActive(false);
    }

    private void AddRightClickHistory(GameObject btnGO, string mapId, int index)
    {
        var trigger = btnGO.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btnGO.AddComponent<EventTrigger>();
        if (trigger.triggers == null) trigger.triggers = new List<EventTrigger.Entry>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            var ped = data as PointerEventData;
            if (ped == null) return;

            if (ped.button == PointerEventData.InputButton.Right)
            {
                ShowHistory(mapId, index);
            }
        });

        trigger.triggers.Add(entry);
    }

    private void ShowHistory(string mapId, int index)
    {
        if (_historyPanel == null || _historyText == null)
        {
            string fallback = GetHistoryText(mapId, index);
            Debug.Log($"[MapSelector] History ({mapId}): {fallback}");
            return;
        }

        string text = GetHistoryText(mapId, index);

        // ============================
        // ✅ NEW: toggle（同一张右键再次关闭）
        // ============================
        if (_historyPanel.activeSelf && _historyText.text == text)
        {
            _historyPanel.SetActive(false);
            return;
        }

        _historyText.text = text;
        _historyPanel.SetActive(true);
    }

    private string GetHistoryText(string mapId, int index)
    {
        if (mapId.Equals("Daba", StringComparison.OrdinalIgnoreCase))
        {
            return "Daba: Defend the strategic dam from an enemy landing. Losing control would cause widespread destruction.";
        }

        if (mapId.Equals("Zongcai", StringComparison.OrdinalIgnoreCase))
        {
            return "Zongcai: Defend the Chairman’s headquarters against enemy forces seeking to destroy leadership.";
        }

        return $"{mapId}: (no history text set)";
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

        if (mapPreviewPaths.Count < mapIds.Count)
        {
            Debug.LogWarning($"[MapSelector] mapPreviewPaths ({mapPreviewPaths.Count}) < mapIds ({mapIds.Count}). Some maps may have no preview sprite.");
        }

        if (mapHistories == null) mapHistories = new List<string>();
    }
}
