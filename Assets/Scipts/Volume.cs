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

    private MusicPlayer _player;

    private void Awake()
    {
        if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider)  musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (musicToggle)  musicToggle.onValueChanged.AddListener(OnMusicToggle);
        if (musicDropdown) musicDropdown.onValueChanged.AddListener(OnSelectSong);

        FindPlayer();
        RefreshUI();
    }

    private void OnEnable()
    {
        FindPlayer();
        RefreshUI();
        GameHotkeys.SettingsChanged += RefreshUI;
    }

    private void OnDisable()
    {
        GameHotkeys.SettingsChanged -= RefreshUI;
    }

    private void FindPlayer()
    {
        // 允许 inactive 也能找到
        _player = UnityEngine.Object.FindFirstObjectByType<MusicPlayer>(FindObjectsInactive.Include);
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

        if (_player != null)
        {
            _player.SetMusicVolume(music);
            _player.SetMusicOn(on);
        }

        BuildDropdownOptions();

        if (musicDropdown)
        {
            int count = (_player != null) ? _player.SongCount() : 0;
            if (count > 0)
            {
                index = Mathf.Clamp(index, 0, count - 1);
                musicDropdown.SetValueWithoutNotify(index);
            }
            else
            {
                musicDropdown.SetValueWithoutNotify(0);
            }
        }
    }

    private void BuildDropdownOptions()
    {
        if (!musicDropdown) return;

        FindPlayer();
        if (_player == null) return;

        int count = _player.SongCount();
        if (count <= 0) return;

        musicDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < count; i++)
            options.Add(_player.GetSongName(i));

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

        FindPlayer();
        if (_player != null)
            _player.SetMusicVolume(v);
    }

    private void OnMusicToggle(bool on)
    {
        PlayerPrefs.SetInt(PREF_ON, on ? 1 : 0);
        PlayerPrefs.Save();

        FindPlayer();
        if (_player != null)
            _player.SetMusicOn(on);
    }

    private void OnSelectSong(int index)
    {
        PlayerPrefs.SetInt(PREF_INDEX, index);
        PlayerPrefs.Save();

        FindPlayer();
        if (_player != null)
            _player.PlaySong(index);
    }
}
