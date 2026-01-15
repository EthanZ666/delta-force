using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour
{
    public enum Mode
    {
        MapSelectUI,
        ApplyInGameplay
    }

    public Mode mode = Mode.MapSelectUI;

    public string gameplaySceneName = "SampleScene";

    public string[] mapIds = { "Daba", "Zongcai" };
    public string[] mapPreviewPaths = { "Images/Daba", "Images/Zongcai" };

    public Vector2 buttonSize = new Vector2(520, 340);
    public float spacing = 650f;

    public string fallbackMapId = "Daba";
    public GameObject[] mapRoots;

    public static string PrefKey => "selected_map_id";

    private void Start()
    {
        if (mode == Mode.MapSelectUI)
        {
            CreateUI();
        }
        else
        {
            ApplyMap();
        }
    }

    private void CreateUI()
    {
        if (mapIds == null || mapPreviewPaths == null || mapIds.Length == 0 || mapIds.Length != mapPreviewPaths.Length)
        {
            Debug.LogError("MapSelector: mapIds and mapPreviewPaths must exist and have the same length.");
            return;
        }

        var canvasGO = new GameObject("MapSelectCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        float startX = -spacing * (mapIds.Length - 1) / 2f;

        for (int i = 0; i < mapIds.Length; i++)
        {
            string id = mapIds[i];
            string path = mapPreviewPaths[i];

            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"MapSelector: Missing preview at Resources/{path}.png");
                continue;
            }

            var go = new GameObject($"Map_{id}");
            go.transform.SetParent(canvasGO.transform, false);

            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => SelectMapAndPlay(id));

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0);
            rt.sizeDelta = buttonSize;
        }
    }

    private void SelectMapAndPlay(string mapId)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError("MapSelector: mapId is empty.");
            return;
        }

        PlayerPrefs.SetString(PrefKey, mapId);
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError($"MapSelector: Cannot load '{gameplaySceneName}'. Add it to Build Settings.");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void ApplyMap()
    {
        if (mapRoots == null || mapRoots.Length == 0)
        {
            Debug.LogError("MapSelector: mapRoots is empty (Assign your map parent objects in SampleScene).");
            return;
        }

        string selected = PlayerPrefs.GetString(PrefKey, fallbackMapId);

        for (int i = 0; i < mapRoots.Length; i++)
        {
            if (mapRoots[i] != null) mapRoots[i].SetActive(false);
        }

        int chosen = 0;
        for (int i = 0; i < mapIds.Length; i++)
        {
            if (mapIds[i] == selected)
            {
                chosen = i;
                break;
            }
        }

        if (chosen >= 0 && chosen < mapRoots.Length && mapRoots[chosen] != null)
        {
            mapRoots[chosen].SetActive(true);
        }
    }

    private void EnsureEventSystem()
    {
        var es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es != null) return;

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }
}
