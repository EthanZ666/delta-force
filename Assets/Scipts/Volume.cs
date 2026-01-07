using UnityEngine;

public class Volume : MonoBehaviour
{
    public Slider volumeSlider()
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

