using System;
using System.Collections;
using UnityEngine;

public abstract class BaseInteractionObject : MonoBehaviour, IPullable, IRespawnable
{
    [Header("플레이어와 장애물 간의 상호작용 가능 거리")] 
    [SerializeField] protected float _linkDist = 0.5f;
    
    [Header("오브젝트 재생성 대기시간 설정")] 
    [SerializeField] protected float _respawnTime = 1f;
    
    protected Vector2 _spawnPos;
    protected Transform _playerHand;
    public Rigidbody2D _rb;
    protected SpriteRenderer _renderer;
    protected Collider2D _collider;
    protected bool _isPulling = false;
    protected bool _isRespawning = false;

    public virtual void Update()
    {
        if (_isPulling && _playerHand != null)
        {
            Vector2 grabPoint = _collider.ClosestPoint(_playerHand.position);
            float dist = Vector2.Distance(grabPoint, _playerHand.position);

            if (dist > _linkDist)
            {
                OnStopP();
            }
        }
    }

    public virtual void Init()
    {
        _rb = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _spawnPos = transform.position;
        
        PullingState(false);
    }
    
    public virtual void OnPull(Transform playerHand)
    {
        _isPulling = true;
        _playerHand = playerHand;
        PullingState(_isPulling);
    }

    public virtual void OnStopP()
    {
        if (_playerHand != null)
        {
            var player = _playerHand.GetComponentInParent<PlayerController>();
            player?.OffGrab();
        }
        _isPulling = false;
        _playerHand = null;
        PullingState(_isPulling);
    }

    public virtual void Respawn()
    {
        if (_isRespawning) return;
        OnStopP();
        StartCoroutine(RespawnRoutine());
    }

    public virtual void PullingState(bool isPulling)
    {
        if (isPulling) _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        else           _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    public virtual void RespawningState(bool isEnabled)
    {
        _rb.simulated = isEnabled;
        _renderer.enabled = isEnabled;
        _collider.enabled = isEnabled;
    }

    public virtual IEnumerator RespawnRoutine()
    {
        _isRespawning = true;
        RespawningState(false);
        
        yield return YieldContainer.WaitForSeconds(_respawnTime) ;
        
        transform.position = _spawnPos;
        _rb.linearVelocity = Vector2.zero;
        
        RespawningState(true);
        PullingState(false);
        _isRespawning = false;
    }
}
