using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuOverlayUI : MonoBehaviour
{
    private void Start()
    {
        // 自动加载背景图：路径是 Resources/Images/MainMenuScene.png
        Sprite backgroundSprite = Resources.Load<Sprite>("Images/MainMenuScene");

        if (backgroundSprite == null)
        {
            Debug.LogError("MainMenu images not found");
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

        // 隐形 Start 按钮（叠在图上的 START 位置）
        Button startButton = CreateTransparentButton(new Vector2(0, -100), new Vector2(240, 80), () =>
        {
            SceneManager.LoadScene("MapSelectScene"); // 替换为你地图选择的场景名
        });
        startButton.transform.SetParent(canvasGO.transform, false);
    }

    Button CreateTransparentButton(Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("StartButton");
        var btn = btnObj.AddComponent<Button>();
        var img = btnObj.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0); // 完全透明

        var rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        btn.onClick.AddListener(onClick);
        return btn;
    }
}
