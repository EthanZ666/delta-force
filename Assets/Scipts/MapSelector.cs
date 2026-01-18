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
    public RectTransform targetArea;
    public string autoFindTargetAreaName = "ScenarioSlots";

    public Vector2 buttonSize = new Vector2(520, 320);
    public float spacing = 80f;

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
            if (go != null)
                targetArea = go.GetComponent<RectTransform>();
        }

        BuildUI();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsCurrentScene(mapSelectSceneName))
        {
            DestroyGeneratedUI();
            Destroy(gameObject);
        }
    }

    // ============================
    // Core UI logic
    // ============================
    private void BuildUI()
    {
        DestroyGeneratedUI();

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
            if (c.name.StartsWith("MapBtn_", StringComparison.OrdinalIgnoreCase))
            {
                Destroy(c.gameObject);
            }
        }

        // 2️⃣ 确保 HorizontalLayoutGroup
        var hlg = parent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
            hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();

        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = spacing;

        // 3️⃣ 强制 ScenarioSlots 尺寸（防止丢图）
        int count = Mathf.Max(1, mapIds.Count);
        float width = count * buttonSize.x + (count - 1) * spacing;
        float height = buttonSize.y;

        parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
        parent.pivot = new Vector2(0.5f, 0.5f);
        parent.sizeDelta = new Vector2(width, height);

        // 4️⃣ 生成按钮
        for (int i = 0; i < mapIds.Count; i++)
        {
            string id = mapIds[i];
            string path = i < mapPreviewPaths.Count ? mapPreviewPaths[i] : "";

            var btnGO = new GameObject($"MapBtn_{id}");
            btnGO.transform.SetParent(parent, false);

            var rect = btnGO.AddComponent<RectTransform>();
            rect.sizeDelta = buttonSize;

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredWidth = buttonSize.x;
            le.preferredHeight = buttonSize.y;
            le.minWidth = buttonSize.x;
            le.minHeight = buttonSize.y;

            var img = btnGO.AddComponent<Image>();
            img.preserveAspect = true;

            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
                img.sprite = sprite;
            else
                Debug.LogWarning($"[MapSelector] Missing sprite: Resources/{path}.png");

            var btn = btnGO.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => SelectMapAndLoad(id));
        }
    }

    // ============================
    // Map select / apply
    // ============================
    private void SelectMapAndLoad(string mapId)
    {
        PlayerPrefs.SetString(selectedMapKey, mapId);
        PlayerPrefs.Save();

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError($"Scene not in Build Settings: {gameplaySceneName}");
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
}
