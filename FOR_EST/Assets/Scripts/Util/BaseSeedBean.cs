using UnityEngine;

public abstract class BaseSeedBean : MonoBehaviour
{
    protected Animator _anim;
    protected SpriteRenderer _renderer;

    public virtual void Init()
    {
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        
        if (transform.position.y < -1)
        {
            _anim.SetBool("Reverse", true);
            _renderer.flipY = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + 1f);

        }
    }
}
