using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public AudioSource musicSource; // BGM을 재생할 AudioSource
    public Slider musicSlider; // BGM 볼륨 조절을 위한 Slider

    public AudioSource buttonsSource;
    public void SetMusicVolume(float volume)
    { 
        musicSource.volume = volume;
    }

    public void SetButtonVolume(float volume)
    { 
        musicSource.volume = volume;
    }

    public void Onsfx()
    { 
        buttonsSource.Play();
    }

    public void DecreaseVolume()
    {
        musicSource.volume -= 0.05f;
        musicSlider.value = musicSource.volume;
    }

    public void IncreaseVolume()
    {
        musicSource.volume += 0.05f;
        musicSlider.value = musicSource.volume;
    }

}
