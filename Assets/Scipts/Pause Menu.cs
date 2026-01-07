// using UnityEngine;
// using UnityEngine.SceneManagement; 

// public class PauseMenu : MonoBehaviour
// {
//     [Header("Pause Panel")]
//     [SerializeField] private GameObject pausePanel;

//     [Header("Scene")]
//     [SerializeField] private string startSceneName = "MainMenu";

//     private bool isPaused = false;

//     private void Start()
//     {
//         if (pausePanel != null)
//             pausePanel.SetActive(false);
//     }

//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             if (isPaused)
//                 ResumeGame();
//             else
//                 PauseGame();
//         }
//     }

//     private void PauseGame()
//     {
//         isPaused = true;

//         if (pausePanel != null)
//             pausePanel.SetActive(true);

//         Time.timeScale = 0f;

//         Cursor.lockState = CursorLockMode.None;
//         Cursor.visible = true;

//         Debug.Log("Game paused");
//     }

//     public void ResumeGame()
//     {
//         isPaused = false;

//         if (pausePanel != null)
//             pausePanel.SetActive(false);

//         Time.timeScale = 1f;

//         Debug.Log("Game resumed");
//     }

//     public void GoToStartScene()
//     {
//         Time.timeScale = 1f;
//         isPaused = false;

//         if (pausePanel != null)
//             pausePanel.SetActive(false);

//         Debug.Log("Loading start scene: " + startSceneName);
//         SceneManager.LoadScene(startSceneName);
//     }
// }