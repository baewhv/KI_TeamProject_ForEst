using System;
using System.Collections;
using UnityEngine;

public abstract class BaseInteractionObject : MonoBehaviour, IPullable, IRespawnable
{
    [Header("플레이어와 장애물 간의 상호작용 가능 거리")] 
    [SerializeField] protected float _linkDist = 0.5f;
    
    [Header("오브젝트 재생성 대기시간 설정")] 
    [SerializeField] protected float _respawnTime = 1f;
    
    [Header("바닥으로 감지될 거리")]
    [SerializeField] protected float _groundDistance;
        
    [Header("바닥을 감지 할 박스의 x축 크기")]
    [SerializeField] protected float _groundSizeX = 1.02f;
    
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
                OnStopPull();
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

    public virtual void OnStopPull()
    {
        if (_playerHand != null)
        {
            var player = _playerHand.GetComponentInParent<PlayerController>();
            if (player != null) player?.OffGrab();
        }
        _isPulling = false;
        _playerHand = null;
        PullingState(_isPulling);
    }

    public virtual void Respawn()
    {
        gameObject.SetActive(true);
        if (_isRespawning) return;
        OnStopPull();
        _isRespawning = true;
        transform.position = _spawnPos;
        _rb.linearVelocity = Vector2.zero;
        PullingState(false);
        _isRespawning = false;
    }
    
    public virtual void CheckGroundState(out Vector2 origin, out Vector2 checkBoxSize, out float direction)
    {
        direction = Mathf.Sign(_rb.gravityScale);
        float checkY = (direction > 0) ? _collider.bounds.min.y : _collider.bounds.max.y;

        origin = new Vector2(_collider.bounds.center.x, checkY);
        checkBoxSize = new Vector2(_collider.bounds.size.x * _groundSizeX, 0.05f);
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
}
