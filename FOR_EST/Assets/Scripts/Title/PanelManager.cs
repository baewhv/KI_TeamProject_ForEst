using UnityEngine;
using UnityEngine.SceneManagement;
public class PanelManager : MonoBehaviour
{
    public GameObject titlePanel; //TitlePanel의 GameObject를 넣어주세요.
    public GameObject settingsPanel; //SettingsPanel의 GameObject를 넣어주세요.
    public GameObject creditsPanel; //CreditsPanel의 GameObject를 넣어주세요.
    public GameObject soundPanel; //SoundPanel의 GameObject를 넣어주세요.
    public GameObject escPanel; //escPanel의 GameObject를 넣어주세요.
    public GameObject masterPanel; //masterPanel의 GameObject를 넣어주세요.
    public GameObject musicPanel; //musicPanel의 GameObject를 넣어주세요.
    public GameObject sfxPanel; //sfxPanel의 GameObject를 넣어주세요.
    public GameObject koreaPanel; //koreaPanel의 GameObject를 넣어주세요.
    public GameObject engPanel; //engPanel의 GameObject를 넣어주세요.
    public GameObject jpPanel; //jpPanel의 GameObject를 넣어주세요.


    public void NewGame()
    {
        SceneManagement.Instance.LoadScene("StageT");
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

    public void CloseEscPanel()
    {
        escPanel.SetActive(false);

        if (Time.timeScale == 0) //멈춰있으면
        {
            Time.timeScale = 1f; //시작
        }
        else //움직이면
        {
            Time.timeScale = 0; //멈추기
        }
    }

    public void OpenMusicPanel()
    {
        masterPanel.SetActive(false);
        musicPanel.SetActive(true);
        sfxPanel.SetActive(false);
    }

    public void OpenSFXPanel()
    {
        musicPanel.SetActive(false);
        sfxPanel.SetActive(true);
        masterPanel.SetActive(false);
    }

    public void OpenMasterPanel()
    {
        sfxPanel.SetActive(false);
        masterPanel.SetActive(true);
        musicPanel.SetActive(false);
    }

    public void OpenKoreaPanel()
    {
        koreaPanel.SetActive(true);
        engPanel.SetActive(false);
        jpPanel.SetActive(false);
    }

    public void OpenEngPanel()
    {
        koreaPanel.SetActive(false);
        engPanel.SetActive(true);
        jpPanel.SetActive(false);
    }

    public void OpenJPPanel()
    {
        koreaPanel.SetActive(false);
        engPanel.SetActive(false);
        jpPanel.SetActive(true);
    }

    public void Quit()
    {
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false; //유니티 에디터에서 Play만 비활성화 시키는 함수
#else
        Application.Quit(); //실제 빌드에서는 게임 종료 시키려면 이 함수 활성화 해야 함
        
#endif
    }
}