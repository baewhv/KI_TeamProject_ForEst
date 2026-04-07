using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SeedController : MonoBehaviour
{
    
    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        if (transform.position.y < -1)
        {
            _anim.SetTrigger("Reverse");
            _renderer.flipY = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + 1.6f);
        }
    }
}
