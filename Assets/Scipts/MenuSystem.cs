using UnityEngine;

public class MenuSystem
{
    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Scene.Manager.LoadScene(filler);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    public void Return_Home()
    {
        SceneManager.LoadScene(filler);
    }

}
