using TMPro;
using UnityEngine;

public abstract class BaseSeedBean : MonoBehaviour
{
    protected Animator _anim;
    protected SpriteRenderer _renderer;
    protected GameObject textBox;
    protected Canvas textBoxCanvas;
    protected TextMeshProUGUI _text;

    public virtual void Init()
    {
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        textBoxCanvas = GetComponentInChildren<Canvas>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        
        if (textBoxCanvas != null)
        {
            textBox = textBoxCanvas.gameObject;
            textBox.SetActive(false);
        }
        
        if (transform.position.y < -1)
        {
            _anim.SetBool("Reverse", true);
            _renderer.flipY = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + 1f);
            textBox.transform.position = new Vector2(transform.position.x, transform.position.y - 2.5f);
        }
    }
    
    public virtual bool RandomSetBool()
    {
        int check = UnityEngine.Random.Range(0, 1);
        if(check == 0) return true;
        return false;
    }
}
