using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
public class LocalizationTest : MonoBehaviour
{
    int currentIndex = 0; //현재 언어를 0번으로 초기화
    int maxIndex = 2; //언어가 3개 있으므로 최대 인덱스는 2, 0:한국어, 1:영어, 2:일본어

    public void NextLanguage()
    {
        currentIndex++; //다음 언어로 변경
        if (currentIndex > maxIndex) //인덱스가 최대값을 초과하면

            currentIndex = 0; //인덱스를 초기화하여 다시 한국어로 돌아감

        ApplyLanguage();
    }

    public void PrevLanguage()
    {
        currentIndex--; //이전 언어로 변경
        if (currentIndex < 0) //인덱스가 0보다 작아지면

            currentIndex = maxIndex; //인덱스가 0보다 작아지면 최대값으로 설정하여 일본어로 돌아감

        ApplyLanguage();
    }

    /*public void ChangeLanguagetoKorean() //한국어로 변경
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
    } 이전에 사용했던 언어 변경 함수(26.04.07에 수정됨)*/

    public void ApplyLanguage()
    {
        //LocalizationSettings.SelectedLocale : 현재 언어를 설정하거나 불러오는 프로퍼티
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentIndex];
        
        LanguageSetting.currentLanguage = (Language)currentIndex;

        Dialogue.Instance.SetLanguageIndex(currentIndex);
    }
}
