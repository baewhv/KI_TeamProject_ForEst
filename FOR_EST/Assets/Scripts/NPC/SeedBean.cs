using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedBean : MonoBehaviour, IRespawnable
{
    private Animator _anim;
    private SpriteRenderer _renderer;
    private BoxCollider2D _collider;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        
        if (transform.position.y < -1)
        {
            _anim.SetBool("Reverse", true);
            _renderer.flipY = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + 1f);
            Vector2 colliderPosY = _collider.offset;
            colliderPosY.y = _collider.offset.y - 1f;
            _collider.offset = colliderPosY;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Happy"))
        {
            if (SceneManager.GetActiveScene().name != "StageT")
            {
                Vector2 direction = (other.transform.position - transform.position).normalized;
                float dot = Vector2.Dot(transform.right, direction);

                _renderer.flipX = dot > 0 ? true : false;
            }
            
            BeHandedFruit();
            StartCoroutine(delayCoroutine());
        }
        
    }

    private void BeHandedFruit()
    {
        _anim.SetTrigger("Put");
    }

    private IEnumerator delayCoroutine()
    {
        yield return YieldContainer.WaitForSeconds(1f);
        _collider.enabled = false;
        _anim.SetBool("Clear", true);
    }

    public void Respawn()
    {
        _collider.enabled = true;
        _anim.SetBool("Clear", false);
    }
}
