using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    public AudioSource musicPlayer;
    public AudioClip[] songs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 防止同场景/跨场景重复
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicPlayer == null)
            musicPlayer = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 你原本的 Start 播放逻辑保留即可
        if (musicPlayer != null && songs != null && songs.Length > 0 && musicPlayer.clip == null)
        {
            musicPlayer.clip = songs[0];
            musicPlayer.loop = true;
            musicPlayer.Play();
        }
    }

    public AudioSource GetSource() => musicPlayer;
}
