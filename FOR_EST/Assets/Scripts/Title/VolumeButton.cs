using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeButton : MonoBehaviour
{
    public Slider musicSlider; // BGM 볼륨 조절을 위한 Slider
    public AudioSource buttonSource;
    public void Onsfx()
    { 
        buttonSource.Play();
    }

    public void DecreaseVolume()
    {
        musicSlider.value -= 0.05f;
    }

    public void IncreaseVolume()
    {
        musicSlider.value += 0.05f;
    }

}