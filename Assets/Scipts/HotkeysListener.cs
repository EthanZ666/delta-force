using UnityEngine;

public class HotkeysListener : MonoBehaviour
{
    private void Awake()
    {
        var all = FindObjectsByType<HotkeysListener>(FindObjectsSortMode.None);
        if (all.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        GameHotkeys.Tick();
    }
}
