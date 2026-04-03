using UnityEngine;

public class SeedBean1 : BaseSeedBean
{
    private GameObject textBox;
    private Canvas textBoxCanvas;

    private void Awake()
    {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 감지");
            textBox.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            textBox.SetActive(false);
        }
    }

    private void Init()
    {
        base.Init();
        
        // _anim.SetBool("Sit", true);
        
        textBoxCanvas = GetComponentInChildren<Canvas>();
        if (textBoxCanvas != null)
        {
            textBox = textBoxCanvas.gameObject;
            textBox.SetActive(false);
        }
    }
}
