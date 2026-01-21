using UnityEngine;
using UnityEngine.UI;

public class SettingScene : MonoBehaviour
{
    private bool isOff;
    public Slider volumeSlider;
    private float savedVolume = 1f;
    

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("volume", 1f);
        AudioListener.volume = volume;
        volumeSlider.value = volume;
        if (volume == 0f)
        {
            isOff = true;
        }
        else
        {
            isOff = false;
        }

        if (!isOff)
        {
            savedVolume = volume;
        }
    }


    public void ToggleOnOff()
    {
        if (isOff)
        {
            AudioListener.volume = savedVolume;
            volumeSlider.value = savedVolume;
            PlayerPrefs.SetFloat("volume", savedVolume);
            isOff = false;
        }
        else
        {
            savedVolume = AudioListener.volume;
            AudioListener.volume = 0f;
            volumeSlider.value = 0f;
            PlayerPrefs.SetFloat("volume", 0f);
            isOff = true;
        }
    }
    
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("volume", value);
         if (AudioListener.volume == 0f)
        {
            isOff = true;
        }
        else
        {
            isOff = false;
            savedVolume = value;
        }
    }
}
