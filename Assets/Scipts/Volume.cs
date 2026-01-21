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

    private MusicPlayer _player;

    private void Awake()
    {
        if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider)  musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (musicToggle)  musicToggle.onValueChanged.AddListener(OnMusicToggle);

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
        // 允许 inactive 也能找到（比如 DontDestroyOnLoad 的对象）
        _player = UnityEngine.Object.FindFirstObjectByType<MusicPlayer>(FindObjectsInactive.Include);
    }

    private void RefreshUI()
    {
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
        float music  = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
        bool on      = PlayerPrefs.GetInt(PREF_ON, 1) == 1;

        if (masterSlider) masterSlider.SetValueWithoutNotify(master);
        if (musicSlider)  musicSlider.SetValueWithoutNotify(music);
        if (musicToggle)  musicToggle.SetIsOnWithoutNotify(on);

        // Apply immediately
        AudioListener.volume = master;

        FindPlayer();
        if (_player != null)
        {
            _player.SetMusicVolume(music);
            _player.SetMusicOn(on);
        }
        else
        {
            Debug.LogWarning("[Volume] MusicPlayer not found in scene (or DontDestroyOnLoad).");
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

        FindPlayer();
        if (_player != null)
            _player.SetMusicVolume(v);
        else
            Debug.LogWarning("[Volume] MusicPlayer not found when changing music volume.");
    }

    private void OnMusicToggle(bool on)
    {
        PlayerPrefs.SetInt(PREF_ON, on ? 1 : 0);
        PlayerPrefs.Save();

        FindPlayer();
        if (_player != null)
            _player.SetMusicOn(on);
        else
            Debug.LogWarning("[Volume] MusicPlayer not found when toggling music.");
    }
}
