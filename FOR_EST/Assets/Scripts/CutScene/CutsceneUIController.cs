using UnityEngine;
using UnityEngine.UI;

public class CutsceneUIController : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fader;
    [SerializeField] private GameObject backgroundGO;
    
    public Image BackgroundImage => backgroundImage;
    public Image Fader => fader;

    public void OnBackground()
    {
        backgroundGO.SetActive(true);
    }

    public void OffBackground()
    {
        backgroundGO.SetActive(false);
    }
    
    
}
