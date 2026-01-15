// using UnityEngine;
// using UnityEngine.UI;

// public class Volume : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private Slider masterSlider;
//     [SerializeField] private Slider musicSlider;
//     [SerializeField] private Toggle musicToggle;

//     private const string PREF_MASTER = "volume_master";
//     private const string PREF_MUSIC = "volume_music";
//     private const string PREF_MUSIC_ON = "music_on";

//     private void Awake()
//     {
//         // 绑定回调
//         if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMaster);
//         if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusic);
//         if (musicToggle != null) musicToggle.onValueChanged.AddListener(SetMusicOn);

//         RefreshFromPrefs();
//         Debug.Log("Volume initialized");
//     }

//     private void OnEnable()
//     {
//         // 每次进入 SettingsScene / 打开面板都刷新一次
//         RefreshFromPrefs();

//         // Bug5：Hotkeys 改了音量，UI 跟着刷新
//         GameHotkeys.SettingsChanged += RefreshFromPrefs;
//     }

//     private void OnDisable()
//     {
//         GameHotkeys.SettingsChanged -= RefreshFromPrefs;
//     }

//     private void RefreshFromPrefs()
//     {
//         float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
//         float music = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
//         bool on = PlayerPrefs.GetInt(PREF_MUSIC_ON, 1) == 1;

//         if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
//         if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
//         if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(on);

//         ApplyMaster(master);
//         ApplyMusic(music);
//         ApplyMusicOn(on);
//     }

//     public void SetMaster(float value)
//     {
//         value = Mathf.Clamp01(value);
//         ApplyMaster(value);
//         PlayerPrefs.SetFloat(PREF_MASTER, value);
//         PlayerPrefs.Save();
//     }

//     public void SetMusic(float value)
//     {
//         value = Mathf.Clamp01(value);
//         ApplyMusic(value);
//         PlayerPrefs.SetFloat(PREF_MUSIC, value);
//         PlayerPrefs.Save();
//     }

//     public void SetMusicOn(bool on)
//     {
//         ApplyMusicOn(on);
//         PlayerPrefs.SetInt(PREF_MUSIC_ON, on ? 1 : 0);
//         PlayerPrefs.Save();
//     }

//     private void ApplyMaster(float v)
//     {
//         AudioListener.volume = Mathf.Clamp01(v);
//     }

//     private void ApplyMusic(float v)
//     {
//         var src = GetMusicSource();
//         if (src != null) src.volume = Mathf.Clamp01(v);
//     }

//     private void ApplyMusicOn(bool on)
//     {
//         var src = GetMusicSource();
//         if (src != null) src.mute = !on;
//     }

//     private AudioSource GetMusicSource()
//     {
//         // Bug3：不依赖拖引用，直接用 Instance
//         if (MusicPlayer.Instance == null) return null;
//         return MusicPlayer.Instance.GetSource();
//     }
// }
