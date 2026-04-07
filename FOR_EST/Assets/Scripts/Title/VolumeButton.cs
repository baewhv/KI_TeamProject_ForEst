using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeButton : MonoBehaviour
{
    public Slider musicSlider; // BGM 볼륨 조절을 위한 Slider
    public AudioSource gameBooting;
    public AudioSource game_Start;
    public AudioSource Click;
    public AudioSource menu_Insert;
    public AudioSource back;
    public void gameBootingSound()
    {
        gameBooting.Play();
    }

    public void gameStartSound()
    {
        game_Start.Play();
    }

    public void clickSound()
    {
        Click.Play();
    }

    public void menuInsertSound()
    {
        menu_Insert.Play();
    }

    public void backSound()
    {
        back.Play();
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