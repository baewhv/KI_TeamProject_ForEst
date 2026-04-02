using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedBean : MonoBehaviour
{
    private Animator _anim;
    private SpriteRenderer _renderer;

    private void Awake()
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (SceneManager.GetActiveScene().name == "StageT") return;
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Happy"))
        {
            Vector2 direction = (other.transform.position - transform.position).normalized;
            float dot = Vector2.Dot(transform.right, direction);

            _renderer.flipX = dot > 0 ? true : false;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.IsClear)
        {
            StartCoroutine(delayCoroutine());
        }
    }

    public void BeHandedFruit()
    {
        _anim.SetTrigger("Put");
    }

    private IEnumerator delayCoroutine()
    {
        yield return YieldContainer.WaitForSeconds(0.7f);
        _anim.SetBool("Clear", true);
    }
}
