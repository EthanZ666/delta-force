using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource musicPlayer;
    public AudioClip[] songs;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  
    public void PlayMusic(int index)
    {
        if (songs == null || songs.Length == 0)
        {
            Debug.LogWarning("Songs array is empty.");
            return;
        }

        if (index < 0 || index >= songs.Length)
        {
            Debug.LogWarning("Invalid song index: " + index);
            return;
        }

        musicPlayer.clip = songs[index];
        musicPlayer.Play();

        Debug.Log("Playing song index: " + index);
    }


    private void Start()
    {
        if (songs != null && songs.Length > 0)
        {
            PlayMusic(0);
            Debug.Log("Auto-playing music: index 0");
        }
    else
        {
            Debug.LogWarning("No songs assigned in MusicPlayer.");
        }
    }
}