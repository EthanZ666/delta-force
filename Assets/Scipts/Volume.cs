using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;

    [Header("Music Player")]
    [SerializeField] private MusicPlayer musicPlayerScript;

    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC = "volume_music";
    private const string PREF_MUSIC_ON = "music_on";

    private void Awake()
    {
        float master = PlayerPrefs.GetFloat(PREF_MASTER, 0.8f);
        float music = PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f);
        bool on = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;

        ApplyMaster(master);
        ApplyMusic(music);
        ApplyMusicOn(on);

        if (masterSlider != null) 
            masterSlider.value = master;
        if (musicSlider != null) 
            musicSlider.value = music;
        if (musicToggle != null) 
            musicToggle.isOn = on;

        if (masterSlider != null) 
            masterSlider.onValueChanged.AddListener(SetMaster);
        if (musicSlider != null) 
            musicSlider.onValueChanged.AddListener(SetMusic);
        if (musicToggle != null) 
            musicToggle.onValueChanged.AddListener(SetMusicOn);

        Debug.Log("Volume initialized");
    }

    public void SetMaster(float value)
    {
        value = Mathf.Clamp01(value);
        ApplyMaster(value);
        PlayerPrefs.SetFloat(PREF_MASTER, value);
        PlayerPrefs.Save();
        Debug.Log("Master volume set to " + value);
    }

    public void SetMusic(float value)
    {
        value = Mathf.Clamp01(value);
        ApplyMusic(value);
        PlayerPrefs.SetFloat(PREF_MUSIC, value);
        PlayerPrefs.Save();
        Debug.Log("Music volume set to " + value);
    }

    public void SetMusicOn(bool on)
    {
        ApplyMusicOn(on);
        PlayerPrefs.SetInt(PREF_MUSIC_ON, on ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Music enabled: " + on);
    }

    private void ApplyMaster(float v)
    {
        AudioListener.volume = v;
    }

    private void ApplyMusic(float v)
    {
        var src = GetMusicSource();
        if (src != null) src.volume = v;
    }

    private void ApplyMusicOn(bool on)
    {
        var src = GetMusicSource();
        if (src != null) src.mute = !on;
    }

    private AudioSource GetMusicSource()
    {
        if (musicPlayerScript == null)
        {
            Debug.LogWarning("MusicPlayer reference missing.");
            return null;
        }

        return musicPlayerScript.musicPlayer;
    }
}
