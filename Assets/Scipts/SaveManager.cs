using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string path =
        Application.persistentDataPath + "/save.json";

    // ================= SAVE =================
    public static void SaveGame()
    {
        SaveData data = new SaveData();

        // ===== Progress =====
        GameObject moneyObj = GameObject.FindWithTag("MoneyManager");
        MoneyManager money = moneyObj != null
            ? moneyObj.GetComponent<MoneyManager>()
            : null;

        if (money != null)
            data.money = money.Balance;

        data.totalEnemiesKilled = GameStats.totalEnemiesKilled;

        // ===== Music Settings =====
        if (MusicPlayer.Instance != null)
        {
            data.musicOn = MusicPlayer.Instance.MusicOn;
            data.volume = MusicPlayer.Instance.GetMusicVolume();
            data.musicIndex = MusicPlayer.Instance.CurrentIndex;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Game Saved to " + path);
    }

    // ================= LOAD =================
    public static void LoadGame()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No save file found");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // ===== Progress =====
        GameObject moneyObj = GameObject.FindWithTag("MoneyManager");
        MoneyManager money = moneyObj != null
            ? moneyObj.GetComponent<MoneyManager>()
            : null;

        if (money != null)
            money.SetBalance(data.money);

        GameStats.totalEnemiesKilled = data.totalEnemiesKilled;

        // ===== Music Settings =====
        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.SetMusicVolume(data.volume);
            MusicPlayer.Instance.SetMusicOn(data.musicOn);
            MusicPlayer.Instance.PlayIndex(data.musicIndex);
        }

        Debug.Log("Game Loaded from " + path);
    }
}
