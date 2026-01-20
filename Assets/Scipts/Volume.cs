using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Volume : MonoBehaviour
{
    [Header("UI (TMP)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private TMP_Dropdown musicDropdown;

    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC  = "volume_music";
    private const string PREF_ON     = "music_on";
    private const string PREF_INDEX  = "music_index";

    private void Awake()
    {
        if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider)  musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (musicToggle)  musicToggle.onValueChanged.AddListener(OnMusicToggle);
        if (musicDropdown) musicDropdown.onValueChanged.AddListener(OnSelectSong);

        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
        GameHotkeys.SettingsChanged += RefreshUI;
    }

    private void OnDisable()
    {
        GameHotkeys.SettingsChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
        float music  = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
        bool on      = PlayerPrefs.GetInt(PREF_ON, 1) == 1;
        int index    = PlayerPrefs.GetInt(PREF_INDEX, 0);

        if (masterSlider) masterSlider.SetValueWithoutNotify(master);
        if (musicSlider)  musicSlider.SetValueWithoutNotify(music);
        if (musicToggle)  musicToggle.SetIsOnWithoutNotify(on);

        AudioListener.volume = master;

        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.SetMusicVolume(music);
            MusicPlayer.Instance.SetMusicOn(on);
        }

        BuildDropdownOptions();

        if (musicDropdown)
            musicDropdown.SetValueWithoutNotify(index);
    }

    private void BuildDropdownOptions()
    {
        if (!musicDropdown) return;
        if (MusicPlayer.Instance == null) return;
        if (MusicPlayer.Instance.SongCount <= 0) return;

        musicDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < MusicPlayer.Instance.SongCount; i++)
            options.Add(MusicPlayer.Instance.GetSongName(i));

        musicDropdown.AddOptions(options);
    }

    private void OnMasterChanged(float v)
    {
        v = Mathf.Clamp01(v);
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(PREF_MASTER, v);
        PlayerPrefs.Save();
    }

    private void OnMusicChanged(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PREF_MUSIC, v);
        PlayerPrefs.Save();

        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicVolume(v);
    }

    private void OnMusicToggle(bool on)
    {
        PlayerPrefs.SetInt(PREF_ON, on ? 1 : 0);
        PlayerPrefs.Save();

        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetMusicOn(on);
    }

    private void OnSelectSong(int index)
    {
        PlayerPrefs.SetInt(PREF_INDEX, index);
        PlayerPrefs.Save();

        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.PlayIndex(index);
    }
}
