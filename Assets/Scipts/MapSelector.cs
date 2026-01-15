using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour
{
    public enum ModeType
    {
        MapSelectUI,
        ApplyInGameplay
    }

    [Header("Mode")]
    public ModeType mode = ModeType.MapSelectUI;

    [Header("Scene")]
    public string gameplaySceneName = "SampleScene";
    public string mapSelectSceneName = "MapSelectScene";

    [Header("Maps")]
    public List<string> mapIds = new List<string> { "Daba", "Zongcai" };
    public List<string> mapPreviewPaths = new List<string> { "Images/Daba", "Images/Zongcai" };
    public string fallbackMapId = "Daba";

    [Header("UI Layout")]
    public Vector2 buttonSize = new Vector2(520, 340);
    public float spacing = 650f;

    [Header("Gameplay Map Roots (only for ApplyInGameplay)")]
    public List<GameObject> mapRoots = new List<GameObject>();

    private const string PREF_SELECTED_MAP = "selected_map_id";

    private Canvas _canvas;
    private GameObject _uiRoot;

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
        if (mode == ModeType.MapSelectUI)
        {
            BuildUI();
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
    }

    private bool IsCurrentScene(string sceneName)
    {
        return string.Equals(SceneManager.GetActiveScene().name, sceneName, StringComparison.OrdinalIgnoreCase);
    }

    private string GetSelectedMapId()
    {
        var id = PlayerPrefs.GetString(PREF_SELECTED_MAP, fallbackMapId);
        if (string.IsNullOrWhiteSpace(id)) id = fallbackMapId;
        return id;
    }

    private void SetSelectedMapId(string id)
    {
        PlayerPrefs.SetString(PREF_SELECTED_MAP, id);
        PlayerPrefs.Save();
    }

    private void BuildUI()
    {
        if (mapIds == null || mapPreviewPaths == null || mapIds.Count == 0)
        {
            Debug.LogWarning("MapSelector: mapIds is empty.");
            return;
        }

        if (mapPreviewPaths.Count != mapIds.Count)
        {
            Debug.LogWarning($"MapSelector: mapPreviewPaths.Count ({mapPreviewPaths.Count}) should match mapIds.Count ({mapIds.Count}).");
        }

        DestroyGeneratedUI();

        _uiRoot = new GameObject("__MapSelectorUIRoot");
        _uiRoot.transform.SetParent(transform, false);

        var canvasGO = new GameObject("__MapSelectorCanvas");
        canvasGO.transform.SetParent(_uiRoot.transform, false);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<GraphicRaycaster>();

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        for (int i = 0; i < mapIds.Count; i++)
        {
            string id = mapIds[i];
            string previewPath = (i < mapPreviewPaths.Count) ? mapPreviewPaths[i] : "";

            var btnGO = new GameObject($"MapBtn_{id}");
            btnGO.transform.SetParent(canvasGO.transform, false);

            var rect = btnGO.AddComponent<RectTransform>();
            rect.sizeDelta = buttonSize;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

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
                spr = Resources.Load<Sprite>($"Images/{id}");
            }

            if (spr == null)
            {
                Debug.LogWarning($"MapSelector: Cannot load preview sprite for '{id}'. Path tried: '{previewPath}' and 'Images/{id}'.");
            }
            else
            {
                img.sprite = spr;
                img.preserveAspect = true;
            }

            var button = btnGO.AddComponent<Button>();
            int capturedIndex = i;
            button.onClick.AddListener(() =>
            {
                OnClickMap(mapIds[capturedIndex]);
            });
        }
    }

    private void OnClickMap(string mapId)
    {
        SetSelectedMapId(mapId);
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void DestroyGeneratedUI()
    {
        if (_uiRoot != null)
        {
            Destroy(_uiRoot);
            _uiRoot = null;
            _canvas = null;
        }
    }

    private void ApplySelectedMapToRoots()
    {
        if (mapRoots == null || mapRoots.Count == 0)
        {
            return;
        }

        var selected = GetSelectedMapId();

        for (int i = 0; i < mapRoots.Count; i++)
        {
            if (mapRoots[i] != null)
                mapRoots[i].SetActive(false);
        }

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
