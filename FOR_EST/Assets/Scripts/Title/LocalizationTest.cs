using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
public class LocalizationTest : MonoBehaviour
{
    public void ChangeLanguagetoKorean() //한국어로 변경
    {
        UpdateLocales(0);
    }

    public void ChangeLanguagetoEnglish() //영어로 변경
    {
        UpdateLocales(1);
    }
    public void ChangeLanguagetoJapanese() //일본어로 변경
    {
        UpdateLocales(2);
    }

    public void UpdateLocales(int index)
    {
        //LocalizationSettings.SelectedLocale : 현재 언어를 설정하거나 불러오는 프로퍼티
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}
