using UnityEngine;


public class PauseMenu : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    private void Start()
    {
        // 初始化
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        // ESC 键切换暂停状态
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;

        // 显示暂停菜单
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // 暂停游戏时间
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game paused");
    }

    public void ResumeGame()
    {
        isPaused = false;

        // 隐藏暂停菜单
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // 恢复游戏时间
        Time.timeScale = 1f;


        Debug.Log("Game resumed");
    }
}
