using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Obstacle
{
    /// <summary>
    /// 리스폰시 솟아오르는 부분에 대한 문제점
    ///         1. 상단에 있는 물체 (플레이어)가 장애물이 솟아오를때 위에
    ///            올라타 있는 상태라면 튀어오른다
    ///         2. 장애물이 솟아 오를때 장애물에 크기가 플렛폼 보다 크다면
    ///            아래쪽에 장애물이 뚫려보인다
    ///         3. 장애물이 리스폰 되는 도중 리스폰 키 입력 시 솟아오리지
    ///            않고 즉시 리스폰 되어 버리는 현상 발생
    ///         4.
    ///
    ///         3.1. bool 변수 추가로 재생성 도중 리스폰키 추가 입력 방지
    ///
    /// 슬라임의 크기에 따른 대응? 추가 필요
    /// </summary>
    // 추가적으로 추상클래스에 대해 열매의 추가 기능에 따라 개편 필요
    public class Obstacle : BaseInteractionObject, IReversable
    {
        [Header("오브젝트 재생성 대기시간 설정")] 
        [SerializeField] private float _respawnTime;

        [Header("플레이어와 장애물 간의 상호작용 가능 거리")] 
        [SerializeField] private float _linkDist = 1f;

        [Header("장애물이 플레이어에게 붙어 있을 거리")]
        [SerializeField] private Vector2 _pivot;
        
        [Header("반전 될 오브젝트")]
        [SerializeField] private ReverseObject _reverseObject;

        [Header("장애물이 솟아오르기 위한 위치")] 
        [SerializeField] private float _riseH;
        
        [Header("장애물이 솟아오르기까지 걸리는 시간")] 
        [SerializeField] private float _riseT;
        
        private SpriteRenderer _renderer;
        private Collider2D[] _collider;
        private float _originalGravity;
        private float _fallingGravity = 7f;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            if (_isPulling && _playerHand != null)
            {
                float dist = Vector2.Distance(transform.position, _playerHand.position);

                if (dist > _linkDist)
                {
                    Debug.Log("플레이어와 멀어짐");
                    OnStopP();
                }
            }

            // 테스트 코드 후에 리스폰 조건 생성시 삭제해야함 
            if (Keyboard.current.rKey.wasPressedThisFrame) Respawn();
        }

        private void FixedUpdate()
        {
            if (_isPulling && _playerHand != null)
            {
                float direction = (_playerHand.position.x > transform.position.x) ? -1f : 1f;
                float followTarget = _playerHand.position.x + (Mathf.Abs(_pivot.x) * direction);
                
                _rb.MovePosition(new Vector2(followTarget, _rb.position.y));

                float gravityDirection = Mathf.Sign(_rb.gravityScale);
                
                if (transform.position.y < _spawnPos.y + 0.5f)     _rb.gravityScale = _fallingGravity * gravityDirection;
                else                                               _rb.gravityScale = _originalGravity * gravityDirection;
            }
        }


        public void Init()
        {
            base.Init();
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponents<Collider2D>();
            _originalGravity = _rb.gravityScale;
        }

        public override void OnPull(Transform playerHand)
        {
            base.OnPull(playerHand);
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public override void OnStopP()
        {
            if (_playerHand != null)
            {
                var player = _playerHand.GetComponentInParent<PlayerController>();
                if (player != null) player.OffGrab();
            }

            base.OnStopP();
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }

        public void Reverse()
        {
            Debug.Log($"Reverse 호출{_isPulling}");
            if (!_reverseObject.canReverse || !_isPulling) return; //당겨지고 있지 않으면 리버스가 안되도록 함

            transform.position *= new Vector2(1f, -1f);
            transform.localScale *= new Vector2(1f, -1f);
            _rb.gravityScale *= -1f;
        }

        // 장애물이 사라지는 조건 (맵 밖으로 밀려남)에 위치에 장애물이 걸리게 되면 해당 메서드를 실행하면 됨
        public override void Respawn()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            _rb.simulated = false;
            _renderer.enabled = false;
            _collider[0].enabled = false;
            _collider[1].enabled = false;
            
            yield return YieldContainer.WaitForSeconds(_respawnTime);
            
            base.Respawn();
            
            _rb.simulated = true;
            _renderer.enabled = true;
            _collider[0].enabled = true;
            _collider[1].enabled = true;
        }


        private void OnDrawGizmos()
        {
            if (_playerHand != null)
            {
                float dist = Vector2.Distance(transform.position, _playerHand.position);

                if (dist > _linkDist) Gizmos.color = Color.red;
                else                  Gizmos.color = Color.green;

                Gizmos.DrawLine(transform.position, _playerHand.position);
                Vector3 cubeSize = new Vector3(_linkDist * 2, _linkDist * 2, 0.1f);

                Gizmos.color = new Color(1, 1, 0, 0.3f); 
                Gizmos.DrawWireCube(transform.position, cubeSize);
            }
        }
    }
}
