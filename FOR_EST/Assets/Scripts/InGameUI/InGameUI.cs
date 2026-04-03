using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class InGameUI : MonoBehaviour
{
    public GameObject escPanel;

    public DialogueTest dialogue;


    public void PressEsc(InputAction.CallbackContext context) //esc를 눌렀을 때 escPanel이 꺼져 있다면 켜지고 켜져있었다면 꺼지는 함수
    {
        if (context.performed)
        {
            escPanel.SetActive(!escPanel.activeSelf);
        }

        if (Time.timeScale == 0) //멈춰있으면
        {
            Time.timeScale = 1f; //시작
        }
        else //움직이면
        {
            Time.timeScale = 0; //멈추기
        }
    }

    public void PrevLanguage() // 이전 언어로 넘어가는 함수
    {
        dialogue.languageIndex--;
    }
    public void NextLanguage() // 다음 언어로 넘어가는 함수
    {
        dialogue.languageIndex++;
    }
}