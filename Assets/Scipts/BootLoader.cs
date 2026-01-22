using UnityEngine;

public class BootLoader : MonoBehaviour
{
    void Start()
    {
        SaveManager.LoadGame();
    }
}
