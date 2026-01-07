using UnityEngine;

public class Volume : MonoBehaviour
{
    public Slider volumeSlider()
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    // Made with the help of chatgpt
        float volumeAmount = PlayerPrefs.GetFloat("volume", 1f);
        AudioListener.volume = volumeAmount;
        volumeSlider.value = AudioListener.volume;
        
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("volume", value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

