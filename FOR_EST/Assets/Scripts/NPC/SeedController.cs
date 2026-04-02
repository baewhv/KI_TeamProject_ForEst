using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SeedController : MonoBehaviour
{
    
    private Rigidbody2D _rb;
    private Animator _anim;
    private UserInput _input;
    private SpriteRenderer _renderer;
    
    private bool checkTriggerEnter = false;

    private void Awake()
    {
        _input = new UserInput();
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        if (transform.position.y < -1)
        {
            _anim.SetTrigger("Reverse");
            _renderer.flipY = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + 1.6f);
        }
    }
    
    private void OnEnable()
    {
        _input.asset.Enable();
        _input.Player.Grab.performed += StageClearCheck;
    }

    private void OnDisable()
    {
        _input.Player.Grab.performed -= StageClearCheck;
        _input.asset.Disable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            checkTriggerEnter = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            checkTriggerEnter = false;
        }
    }

    private void StageClearCheck(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.IsClear && checkTriggerEnter) SceneManagement.Instance.LoadNextScene();
        else if (checkTriggerEnter)                            Debug.Log("대충 넌 아직 못 지나간다");
        else                                                   return;
    }
    
}
