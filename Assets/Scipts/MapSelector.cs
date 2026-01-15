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
    [Tooltip("Only used when mode = MapSelectUI. The scene name that shows the map selection UI.")]
    public string mapSelectSceneName = "MapSelectScene";

    [Tooltip("Only used when mode = MapSelectUI. The scene name to load after selecting a map.")]
    public string gameplaySceneName = "SampleScene";

    [Header("Map IDs (must match Map Roots order)")]
    public List<string> mapIds = new List<string> { "Daba", "Zongcai" };

    [Header("Preview Sprites (Resources paths, no extension, same order as mapIds)")]
    public List<string> mapPreviewPaths = new List<string> { "Images/Daba", "Images/Zongcai" };

    [Header("PlayerPrefs Key")]
    public string selectedMapKey = "selected_map_id";

    [Header("Fallback Map ID (if prefs missing or invalid)")]
    public string fallbackMapId = "Daba";

    [Header("Gameplay Map Roots (only used when mode = ApplyInGameplay)")]
    [Tooltip("In SampleScene, drag in your map root GameObjects here in the same order as mapIds (e.g., Daba-map, Zongcai-map).")]
    public List<GameObject> mapRoots = new List<GameObject>();

    [Header("UI Layout (MapSelectUI)")]
    public Vector2 buttonSize = new Vector2(520, 320);
    public float spacing = 650f;

    private GameObject _generatedCanvas;

    void Awake()
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
        if (mode == ModeType.MapSelectUI)
        {
            // ✅ Key fix: ensure we have a working EventSystem for UI clicks
            EnsureEventSystem();
            BuildUI();
        }
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
    }

    private bool IsCurrentScene(string sceneName)
    {
        return SceneManager.GetActiveScene().name.Equals(sceneName, StringComparison.OrdinalIgnoreCase);
    }

    private void BuildUI()
    {
        DestroyGeneratedUI();

        // Canvas
        _generatedCanvas = new GameObject("MapSelectCanvas");
        var canvas = _generatedCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = _generatedCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _generatedCanvas.AddComponent<GraphicRaycaster>();

        for (int i = 0; i < mapIds.Count; i++)
        {
            string id = mapIds[i];
            string previewPath = (i < mapPreviewPaths.Count) ? mapPreviewPaths[i] : "";

            var btnGO = new GameObject($"MapBtn_{id}");
            btnGO.transform.SetParent(_generatedCanvas.transform, false);

            var rect = btnGO.AddComponent<RectTransform>();
            rect.sizeDelta = buttonSize;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            float x = (i == 0) ? -spacing * 0.5f : spacing * 0.5f;
            rect.anchoredPosition = new Vector2(x, 0);

            var img = btnGO.AddComponent<Image>();
            img.raycastTarget = true;

            Sprite spr = null;
            if (!string.IsNullOrWhiteSpace(previewPath))
            {
                spr = Resources.Load<Sprite>(previewPath);
            }

            if (spr == null)
            {
                Debug.LogWarning($"Map preview sprite not found at Resources/{previewPath}.png (id={id}).");
            }
            else
            {
                img.sprite = spr;
                img.preserveAspect = true;
            }

            var btn = btnGO.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            btn.onClick.AddListener(() =>
            {
                SelectMapAndLoad(id);
            });
        }
    }

    private void DestroyGeneratedUI()
    {
        if (_generatedCanvas != null)
        {
            Destroy(_generatedCanvas);
            _generatedCanvas = null;
        }
    }

    private void SelectMapAndLoad(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            Debug.LogError("SelectMapAndLoad: mapId is empty.");
            return;
        }

        PlayerPrefs.SetString(selectedMapKey, mapId);
        PlayerPrefs.Save();

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError($"Cannot load scene '{gameplaySceneName}'. Add it to Build Settings.");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void ApplySelectedMapToRoots()
    {
        // disable all first
        if (mapRoots != null)
        {
            for (int i = 0; i < mapRoots.Count; i++)
            {
                if (mapRoots[i] != null) mapRoots[i].SetActive(false);
            }
        }

        string selected = PlayerPrefs.GetString(selectedMapKey, fallbackMapId);
        int idx = mapIds != null ? mapIds.IndexOf(selected) : -1;

        if (idx < 0 || idx >= mapRoots.Count || mapRoots[idx] == null)
        {
            idx = mapIds != null ? mapIds.IndexOf(fallbackMapId) : -1;
        }

        if (idx >= 0 && idx < mapRoots.Count && mapRoots[idx] != null)
        {
            mapRoots[idx].SetActive(true);
        }
    }
}
