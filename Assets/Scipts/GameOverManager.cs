using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverAutoReturn : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [SerializeField] private float waitSeconds = 5f;

    private void Start()
    {
        StartCoroutine(ReturnToMenu());
    }

    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSecondsRealtime(waitSeconds);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
