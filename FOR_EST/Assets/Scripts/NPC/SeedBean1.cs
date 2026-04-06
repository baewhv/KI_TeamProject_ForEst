using UnityEngine;

public class SeedBean1 : BaseSeedBean
{

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
        _anim.SetBool("Sit", true);
        
        if (transform.position.y > 5f)
        {
            _text.text = "높다";
        }
        else if (transform.position.y < -5f)
        {
            _text.text = "낮다";
        }
        else _text.text = "평범하다";
    }
}
