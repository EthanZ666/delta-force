using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Audio Source (auto-get if empty)")]
    [SerializeField] private AudioSource musicSource;

    [Header("Songs (drag AudioClips here)")]
    public AudioClip[] songs;

    // PlayerPrefs keys (keep consistent with your GameHotkeys)
    private const string PREF_MASTER = "volume_master";
    private const string PREF_MUSIC  = "volume_music";
    private const string PREF_ON     = "music_on";
    private const string PREF_INDEX  = "music_index";

    public int SongCount => songs == null ? 0 : songs.Length;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Get / create AudioSource
        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
        // Apply saved master volume
        float master = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 0.8f));
        AudioListener.volume = master;

        // Apply saved music volume + toggle
        float musicVol = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 0.8f));
        bool on = PlayerPrefs.GetInt(PREF_ON, 1) == 1;

        SetMusicVolume(musicVol);
        SetMusicOn(on);

        // Play saved song index
        int index = PlayerPrefs.GetInt(PREF_INDEX, 0);
        PlayIndex(index);
    }

    public AudioSource GetSource() => musicSource;

    public string GetSongName(int index)
    {
        if (songs == null || songs.Length == 0) return "(No Songs)";
        index = Mathf.Clamp(index, 0, songs.Length - 1);
        return songs[index] != null ? songs[index].name : "(Missing Clip)";
    }

    public void PlayIndex(int index)
    {
        // makes sure that the song list and selected song isn't empty and that the index is a valid number
        if (songs == null || songs.Length == 0) return;
        if (index < 0 || index >= songs.Length) return;
        if (songs[index] == null) return;

        // sets the selected song as the current clip
        musicSource.clip = songs[index];

        // Only play when music is ON
        if (!musicSource.mute)
            musicSource.Play();

        PlayerPrefs.SetInt(PREF_INDEX, index);
        PlayerPrefs.Save();
    }

    public void SetMusicOn(bool on)
    {
        musicSource.mute = !on;

        if (on)
        {
            if (musicSource.clip != null && !musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            if (musicSource.isPlaying)
                musicSource.Pause();
        }

        PlayerPrefs.SetInt(PREF_ON, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float v)
    {
        v = Mathf.Clamp01(v);
        musicSource.volume = v;

        PlayerPrefs.SetFloat(PREF_MUSIC, v);
        PlayerPrefs.Save();
    }
}
