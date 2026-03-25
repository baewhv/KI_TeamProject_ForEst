using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    /*Scene 이동 할 때 창이 서서히 밝아지게끔 만들어주는 스크립트입니다. 
     Scene 이동 시 blackOut_Curtain 이미지의 알파값이(투명도) 1에서 0으로 서서히
    줄어들면서 화면이 밝아지는 효과를 줍니다.*/

    /*효과 넣고 싶은 Scene의 UI에 이 스크립트를 넣고 인스펙터에 있는 blackOut_Curtain에 이미지를 넣어주면 같은 효과를 줄 수 있습니다.
     이걸 넣고 다른 UI들이 작동안한다면 넣어준 이미지의 인스펙터에서 Raucast Target 체크 해제 하면 됩니다.*/

    public Image blackOut_Curtain; 
    float blackOut_Curtain_value;
    float blackOut_Curtain_speed;

    private void Start()
    {
        blackOut_Curtain_value = 1.0f;
        blackOut_Curtain_speed = 0.5f;
    }

    private void Update()
    {
        if (blackOut_Curtain != null || blackOut_Curtain_value > 0)
        {
            HideBlackOut_Curtain();
        }
    }

    public void HideBlackOut_Curtain()
    { 
        blackOut_Curtain_value -= Time.deltaTime * blackOut_Curtain_speed;
        blackOut_Curtain.color = new Color(0.0f, 0.0f, 0.0f, blackOut_Curtain_value);
    }
}
