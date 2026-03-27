using UnityEngine;
using UnityEngine.SceneManagement;
public class UIButton : MonoBehaviour
{
    public GameObject titlePanel; //TitlePanel의 GameObject를 넣어주세요.
    public GameObject settingsPanel; //SettingsPanel의 GameObject를 넣어주세요.
    public GameObject creditsPanel; //CreditsPanel의 GameObject를 넣어주세요.
    public GameObject soundPanel; //SoundPanel의 GameObject를 넣어주세요.

  
    public void NewGame()
    {
        SceneManagement.Instance.LoadScene("ConversationUI_SHY");
    }

    public void LoadGame()
    {
        Debug.Log("LoadGame버튼은 아직 미구현입니다!");
    }

    public void OpenTitlePanel()
    {
        titlePanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        soundPanel.SetActive(false);
    }
    public void OpneSettingPanel()
    {
        titlePanel.SetActive(false);
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        soundPanel.SetActive(false);
    }

    public void OpenCreditPanel()
    {
        titlePanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        soundPanel.SetActive(false);
    }

    public void OpenSoundPanel()
    {
        titlePanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        soundPanel.SetActive(true);
    }

    public void Quit()
    {
        UnityEditor.EditorApplication.isPlaying = false; //유니티 에디터에서 Play만 비활성화 시키는 함수
        //Application.Quit(); 실제 빌드에서는 게임 종료 시키려면 이 함수 활성화 해야 함
    }
}