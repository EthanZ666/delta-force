using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    [Header("Pause Menu Panel (optional)")]
    public GameObject pausePanel;



    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }



    public void Pause()
    {
    if (pausePanel != null)
        pausePanel.SetActive(true);

    Time.timeScale = 0f;
    }

    public void Resume()
    {
    if (pausePanel != null)
        pausePanel.SetActive(false);

    Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
        if (pausePanel == null) 
            return;
    
        if (pausePanel.activeSelf)
            Resume();
        else
            Pause();
        }
    }
}