using UnityEngine;

public class SettingScene : MonoBehaviour
{
    private bool isOff

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("volume", 1f);
        AudioListener.volume = volume;
        if (volume == 0f)
        {
            isOff = true;
        }
        else
        {
            isOff = false;
        }
    }


    public void ToggleOnOff()
    {
        isOff = !isOff;
        if (isOff)
        {
            AudioListener.volume = 0f;
            PlayerPrefs.SetFloat("volume", 0f);
        }
        else
        {
            AudioListener.volume = 1f;
            PlayerPrefs.SetFloat("volume", 1f);
        }
    }
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("volume", value);
         if (volume == 0f)
        {
            isOff = true;
        }
        else
        {
            isOff = false;
        }
    }
}

