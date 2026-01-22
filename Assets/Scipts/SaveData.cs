using System;

[Serializable]
public class SaveData
{
    // PROGRESS
    public int money;
    public int totalEnemiesKilled;

    // SETTINGS (音乐)
    public bool musicOn;
    public float volume;
    public int musicIndex;   // ⭐ 新增：当前选中的歌曲
}
