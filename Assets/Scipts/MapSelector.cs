using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem; // for Mouse.current / Keyboard.current
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
    public RectTransform targetArea;
    public string autoFindTargetAreaName = "ScenarioSlots";

    [Header("Button Layout (Map previews)")]
    public Vector2 buttonSize = new Vector2(520, 220);
    public float spacing = 80f;

    [Header("ScenarioSlots Area Control (force by code)")]
    public bool forceTargetAreaRect = true;
    public Vector2 targetAreaSize = Vector2.zero;
    public Vector2 targetAreaAnchoredPos = new Vector2(0f, 80f);

    [Header("Optional: Crop (Mask)")]
    public bool addMaskToButtons = true;

    // ============================
    // History (Right Click)
    // ============================
    [Header("History (Right Click)")]
    public List<string> mapHistories = new List<string>
    {
        "Daba: (write your history here)",
        "Zongcai: (write your history here)"
    };

    public string historyPanelName = "HistoryPanel";
    public string historyTextName = "HistoryText";

    private GameObject _historyPanel;
    private TMP_Text _historyText;

    private GameObject _generatedCanvas;

    // ============================
    // ✅ NEW: PausePanel (ESC)
    // ============================
    [Header("Pause (ESC)")]
    public string pausePanelName = "PausePanel"; // 你的层级里就是 PausePanel
    private GameObject _pausePanel;

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
        SetupPausePanelUI();   // ✅ NEW
        BuildUI();
    }

    private void Update()
    {
        if (mode != ModeType.MapSelectUI) return;

        // ============================
        // ✅ NEW: ESC toggle PausePanel
        // ============================
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            TogglePausePanel();
        }

        // ✅ Right click history
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
#else
        if (Input.GetMouseButtonDown(1))
#endif
        {
            if (TryGetMapButtonUnderMouse(out string mapId, out int index))
            {
                ShowHistory(mapId, index);
            }
        }
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
            SetupPausePanelUI(); // ✅ NEW
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
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var c = parent.GetChild(i);
            if (c != null && c.name.StartsWith("MapBtn_", StringComparison.OrdinalIgnoreCase))
            {
                Destroy(c.gameObject);
            }
        }

        var hlg = parent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();

        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = spacing;

        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        hlg.padding = new RectOffset(0, 0, 0, 0);

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

        for (int i = 0; i < mapIds.Count; i++)
        {
            string id = mapIds[i];
            string path = i < mapPreviewPaths.Count ? mapPreviewPaths[i] : "";
            string mapIdLocal = id;

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
                btn.onClick.AddListener(() => SelectMapAndLoad(mapIdLocal));
            }
            else
            {
                var img = btnGO.AddComponent<Image>();
                ApplySprite(img, id, path);

                var btn = btnGO.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => SelectMapAndLoad(mapIdLocal));
            }
        }
    }

    private void ApplySprite(Image img, string id, string path)
    {
        img.raycastTarget = false;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;

        Sprite sprite = null;
        if (!string.IsNullOrWhiteSpace(path))
            sprite = Resources.Load<Sprite>(path);

        if (sprite != null) img.sprite = sprite;
        else Debug.LogWarning($"[MapSelector] Missing sprite for '{id}': Resources/{path}.png");
    }

    private bool TryGetMapButtonUnderMouse(out string mapId, out int index)
    {
        mapId = null;
        index = -1;

        if (EventSystem.current == null) return false;

        var ped = new PointerEventData(EventSystem.current)
        {
#if ENABLE_INPUT_SYSTEM
            position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero
#else
            position = Input.mousePosition
#endif
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results == null || results.Count == 0) return false;

        for (int r = 0; r < results.Count; r++)
        {
            Transform t = results[r].gameObject.transform;
            while (t != null)
            {
                if (t.name.StartsWith("MapBtn_", StringComparison.OrdinalIgnoreCase))
                {
                    string id = t.name.Substring("MapBtn_".Length);
                    int i = mapIds.IndexOf(id);
                    if (i >= 0)
                    {
                        mapId = id;
                        index = i;
                        return true;
                    }
                }
                t = t.parent;
            }
        }

        return false;
    }

    // ============================
    // History UI
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
        if (textTf != null) _historyText = textTf.GetComponent<TMP_Text>();
        if (_historyText == null) _historyText = _historyPanel.GetComponentInChildren<TMP_Text>(true);

        if (_historyText == null)
        {
            Debug.LogWarning($"[MapSelector] No TMP_Text found. Create a 'TextMeshPro - Text (UI)' named '{historyTextName}' under '{historyPanelName}'.");
            return;
        }

        var graphics = _historyPanel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;

        var panelImg = _historyPanel.GetComponent<Image>();
        if (panelImg != null)
            panelImg.color = new Color(panelImg.color.r, panelImg.color.g, panelImg.color.b, 0f);

        _historyPanel.SetActive(false);
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
            return "Zero Dam: Defend the strategic dam from an enemy landing. Losing control would cause widespread destruction.";

        if (mapId.Equals("Zongcai", StringComparison.OrdinalIgnoreCase))
            return "CEO: Defend the Chairman’s headquarters against enemy forces seeking to destroy leadership.";

        return $"{mapId}: (no history text set)";
    }

    // ============================
    // ✅ NEW: PausePanel UI
    // ============================
    private void SetupPausePanelUI()
    {
        _pausePanel = FindSceneObjectByName(pausePanelName);
        if (_pausePanel == null)
        {
            Debug.LogWarning($"[MapSelector] '{pausePanelName}' not found in scene '{SceneManager.GetActiveScene().name}'.");
            return;
        }

        // 你希望默认关着，就保持关着
        // _pausePanel.SetActive(false);

        Debug.Log($"[MapSelector] PausePanel found: {_pausePanel.name} (active={_pausePanel.activeSelf})");
    }

    private void TogglePausePanel()
    {
        if (_pausePanel == null)
        {
            SetupPausePanelUI();
            if (_pausePanel == null) return;
        }

        bool next = !_pausePanel.activeSelf;
        _pausePanel.SetActive(next);

        // 可选：打开暂停时，把 history 关掉避免挡住
        if (next && _historyPanel != null) _historyPanel.SetActive(false);

        Debug.Log($"[MapSelector] ESC -> PausePanel {(next ? "OPEN" : "CLOSE")}");
    }

    private GameObject FindSceneObjectByName(string objName)
    {
        var activeScene = SceneManager.GetActiveScene();
        var all = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < all.Length; i++)
        {
            var t = all[i];
            if (t == null) continue;
            if (t.name != objName) continue;

            var go = t.gameObject;

            // 只要场景里真实物体（排除 prefab/资源）
            if (!go.scene.IsValid()) continue;

            // 只要当前激活场景里的
            if (go.scene != activeScene) continue;

            return go;
        }

        return null;
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
