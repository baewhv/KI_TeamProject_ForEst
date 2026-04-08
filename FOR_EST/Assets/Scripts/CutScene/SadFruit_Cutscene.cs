using System;
using System.Collections;
using UnityEngine;

public class SadFruit_Cutscene : BaseFruitTextData
{

    private SadFruitDissolveFX _dissolveFX;

    private void Awake()
    {
        base.Init();
        _dissolveFX = GetComponent<SadFruitDissolveFX>();

        if (transform.position.y < -1)
        {
            _rb.gravityScale = -1;
            Vector2 scale = transform.localScale;
            scale.y *= -1f;
            transform.localScale = scale;
        }
    }

    private void Start()
    {
        SetDataWithID(2201);
    }


    private void FixedUpdate()
    {
        Debug.Log($"{_isPulling} / {_playerHand}");
        if (_isPulling && _playerHand != null)
        {
            if (!IsGrounded())
            {
                OnStopPull();
                return;
            }

            var playerMovement = _playerHand.GetComponentInParent<CharacterMovement>();
            if (playerMovement != null)
            {
                float playerVX = playerMovement._rigidbody.linearVelocityX;
                _rb.linearVelocity = new Vector2(playerVX, _rb.linearVelocity.y);
            }
        }
    }


    public override void OnStopPull()
    {
        base.OnStopPull();
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        if (_collider == null) return false;

        base.CheckGroundState(out Vector2 origin, out Vector2 checkBoxSize, out float direction);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, checkBoxSize, 0f, Vector2.down * direction, _groundDistance);
        
        foreach (var hit in hits)
        {
            if (hit.collider != null &&
                hit.collider.gameObject != gameObject &&
                !hit.collider.isTrigger &&
                !hit.collider.CompareTag("Player") &&
                !hit.collider.CompareTag("Seed"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.gameObject.CompareTag("Boundary"))
        {
            _dissolveFX?.PlayEffect();
            if (_textRoutine != null) StopCoroutine(_textRoutine);
            _textRoutine = StartCoroutine(TextVisibleRoutine_cutscene());
        }
        else if (target.gameObject.CompareTag("Seed"))
        {
            Respawn();
        }
    }
    public IEnumerator TextVisibleRoutine_cutscene()
    {
        yield return  YieldContainer.WaitForSeconds(0.01f);
        
        _renderer.enabled = false;
        _collider.enabled = false;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePosition; 
        _rb.linearVelocity = Vector2.zero;
        _textBox.SetActive(true);
        _text.ForceMeshUpdate();
        int visibleText = _text.textInfo.characterCount;
        _text.maxVisibleCharacters = 0;

        for (int i = 0; i <= visibleText; i++)
        {
            _text.maxVisibleCharacters = i;
            yield return YieldContainer.WaitForSeconds(_textSpeed);
        }
        
        yield return YieldContainer.WaitForSeconds(1f);
        
        gameObject.SetActive(false);
    }
    
    public override void Respawn()
    {
        if (!gameObject.activeSelf)
        {
            GameManager.Instance.FruitCount++;
        }
        base.Respawn();
    }
}