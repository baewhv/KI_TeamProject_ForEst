using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeText : MonoBehaviour
{
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    void Start()
    {
        UpdateVolumeText();
        volumeSlider.onValueChanged.AddListener(delegate { UpdateVolumeText();});
    }
    public void UpdateVolumeText()
    {
        float percent = Mathf.RoundToInt(volumeSlider.value * 100);
        volumeText.text = percent + "%";
    }
}
