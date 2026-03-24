using System.Collections;
using UnityEngine;

// IPullable 상속
public class SadFruit : MonoBehaviour, IPullable
{
    [SerializeField] private Vector2 _spawnPos;
    [SerializeField] private float _respawnTime = 1f;

    private Rigidbody2D _rb;
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private Transform _playerHand;
    private bool _isPulling = false;

    private void Awake()
    {
        Init();
    }

    private void FixedUpdate()
    {
        if (_isPulling && _playerHand != null)
        {
            _rb.MovePosition(new Vector2(_playerHand.position.x, _rb.position.y));
        }
    }

    public void Init()
    {
        _spawnPos = gameObject.transform.position;
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
        if(_playerHand != null)
        {
            var player = _playerHand.GetComponent<PlayerController>();
            if(player != null) player.OffGrab();
        }

        _isPulling = false;
        _playerHand = null;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    // 무언가와 트리거 했을 때 실행되는 함수
    // target : 부딪힌 상대방을 target이라는 변수명 사용
    private void OnTriggerEnter2D(Collider2D target)
    {
        if(target.CompareTag("Boundary"))
        {
            // 부딪힌 대상이 Boundary라면 열매를 화면에서 사라지게 함(성공)
            gameObject.SetActive(false);
        }

        else if(target.CompareTag("Seed"))
        {
            // 부딪힌 대상이 Seed라면 리셋(실패)
            Respawn();
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
