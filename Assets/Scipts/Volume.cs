using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;

    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC  = "volume_music";
    private const string PREF_ON     = "music_on";

    private void Awake()
    {
        if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider)  musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (musicToggle)  musicToggle.onValueChanged.AddListener(OnMusicToggle);

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

        if (masterSlider) masterSlider.SetValueWithoutNotify(master);
        if (musicSlider)  musicSlider.SetValueWithoutNotify(music);
        if (musicToggle)  musicToggle.SetIsOnWithoutNotify(on);

        AudioListener.volume = master;

        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.SetMusicVolume(music);
            MusicPlayer.Instance.SetMusicOn(on);
        }
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
}
