using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string path =
        Application.persistentDataPath + "/save.json";

    public static void SaveGame()
    {
        SaveData data = new SaveData();

       GameObject moneyObj = GameObject.FindWithTag("MoneyManager");
       MoneyManager money = moneyObj.GetComponent<MoneyManager>();

        if (money != null)
            data.money = money.Balance;

        data.totalEnemiesKilled = GameStats.totalEnemiesKilled;
        // data.musicOn = AudioSettings.musicOn;
        // data.volume = AudioSettings.volume;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Game Saved");
    }

    public static void LoadGame()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No save file found");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        GameObject moneyObj = GameObject.FindWithTag("MoneyManager");
        MoneyManager money = moneyObj.GetComponent<MoneyManager>();

        if (money != null)
            money.SetBalance(data.money);

        GameStats.totalEnemiesKilled = data.totalEnemiesKilled;
        // AudioSettings.musicOn = data.musicOn;
        // AudioSettings.volume = data.volume;

        Debug.Log("Game Loaded");
    }
}
