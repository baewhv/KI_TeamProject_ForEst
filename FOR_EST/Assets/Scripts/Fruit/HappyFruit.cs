using System.Collections;
using UnityEngine;

public class HappyFruit : MonoBehaviour, IPullable
{
    [SerializeField] private Vector2 _spawnPos;
    [Header("상호작용 및 리스폰 설정")]
    [SerializeField] private float _linkDist = 1.2f;
    [SerializeField] private float _respawnTime = 1f;

    [Header("바닥 체크 설정")]
    [SerializeField] private LayerMask _groundLayer; 
    [SerializeField] private float _groundDistance = 0.1f;
    [SerializeField] private float _groundSizeX = 0.3f;

    private Rigidbody2D _rb;
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private Transform _playerHand;
    private bool _isPulling = false;

    private void Awake()
    {
        Init();
    }

    private void Update()
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

    private void FixedUpdate()
    {
        if (_isPulling && _playerHand != null)
        {
            if (!IsGrounded())
            {
                OnStopP();
                return;
            }

            var playerMovement = _playerHand.GetComponentInParent<PlayerMovement>();
            if (playerMovement != null)
            {
                float playerVX = playerMovement._rigidbody.linearVelocityX;

                _rb.linearVelocity = new Vector2(playerVX, _rb.linearVelocity.y);
            }
        }
    }

    public void Init()
    {
        _spawnPos = transform.position;
        _rb = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    public void OnPull(Transform playerHand)
    {
        _isPulling = true;
        _playerHand = playerHand;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void OnStopP()
    {
        if (_playerHand != null)
        {
            var player = _playerHand.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.OffGrab();
            }
        }

        _isPulling = false;
        _playerHand = null;

        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    private bool IsGrounded()
    {
        if (_collider == null)
        {
            return false;
        }

        float direction = Mathf.Sign(_rb.gravityScale);
        float checkY = (direction > 0) ? _collider.bounds.min.y : _collider.bounds.max.y;

        Vector2 origin = new Vector2(_collider.bounds.center.x, checkY);
        Vector2 checkBoxSize = new Vector2(_collider.bounds.size.x * _groundSizeX, 0.05f);

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
            // 부딪힌 대상이 Boundary라면 리셋
            Respawn();
        }
        else if (target.gameObject.CompareTag("Seed"))
        {
            // 부딪힌 대상이 Seed라면 열매를 화면에서 사라지게 함
            gameObject.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.FruitCount--;
        }
    }

    public void Respawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _rb.simulated = false;
        _renderer.enabled = false;
        _collider.enabled = false;

        yield return new WaitForSeconds(_respawnTime);

        transform.position = _spawnPos;
        _rb.linearVelocity = Vector2.zero;

        _rb.simulated = true;
        _renderer.enabled = true;
        _collider.enabled = true;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }
}