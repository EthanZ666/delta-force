using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsBinder : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Slider musicSlider;   // 你的 Music Volume Slider（可选）

    void Start()
    {
        // 兜底：防止主页面暂停遗留导致没声
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        Time.timeScale = 1f;

        var p = MusicPlayer.Instance;
        if (p == null)
        {
            Debug.LogError("[SettingsBinder] MusicPlayer.Instance is null. Put MusicPlayer in the first scene (MainMenu/Splash).");
            return;
        }

        // Dropdown：填选项 + 绑定选歌
        if (dropdown != null)
        {
            p.BindDropdown(dropdown);
            dropdown.SetValueWithoutNotify(p.CurrentIndex);
        }

        // Music Slider：同步显示 + 绑定音量（可选）
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(p.GetMusicVolume());
            musicSlider.onValueChanged.RemoveListener(p.SetMusicVolume);
            musicSlider.onValueChanged.AddListener(p.SetMusicVolume);
        }
    }
}
