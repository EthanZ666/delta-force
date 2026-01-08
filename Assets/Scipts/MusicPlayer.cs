using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource musicPlayer;
    public AudioClip[] songs;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  
    public void PlayMusic(int index)
    {
        if (index < 0 || index >= songs.Length)
            return;

        musicPlayer.clip = songs[index];
        musicPlayer.Play();
    }
