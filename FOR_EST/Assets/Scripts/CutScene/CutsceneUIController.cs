using UnityEngine;
using UnityEngine.UI;

public class CutsceneUIController : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fader;
    
    public Image BackgroundImage => backgroundImage;

    public Image Fader => fader;
}
