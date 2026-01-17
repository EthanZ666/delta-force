using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject panel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";
    [SerializeField] private string settingsScene = "SettingsScene";

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
    }

    // ===== Buttons =====
    public void OnContinue()
    {
        Hide();
        Time.timeScale = 1f; // 保证继续
    }

    public void OnSettings()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(settingsScene);
    }

    public void OnReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
