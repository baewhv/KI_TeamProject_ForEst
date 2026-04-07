using Unity.VisualScripting;
using UnityEngine;

public class ReverseVolume : MonoBehaviour
{
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioSource ReverseAudioSource;

    private void Update()
    {
        float cameraY = Camera.main.transform.position.y;


        if (cameraY > 0)
        {
            AudioSource.mute = false;
            ReverseAudioSource.mute = true;
        }
        else
        {
            AudioSource.mute = true;
            ReverseAudioSource.mute = false;
        }
    }
}