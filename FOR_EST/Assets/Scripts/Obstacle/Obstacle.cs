using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Obstacle
{
    public class Obstacle : MonoBehaviour, IPullable, IReversable
    {
        /// <summary>
        /// 하위 오브젝트로 종속 시
        ///     문제점 1. 키네마틱을 통한 물리연산 무시 시 플레이어는 따라오지만 플레이어의 하위에 발판이 있고
        ///              장애물의 하위에 발판이 없을 시 아래로 떨어지게 만드려면?
        ///           2. 다이나믹을 통한 이동시 물리연산에 의해 플레이어의 하위 오브젝트로는 들어가지만
        ///              실질적으로 장애물은 따로 노는 현상
        ///     해결방안
        ///           1. 그럼 종속 시키지 말고 따라만 다니게 해보자?
        /// </summary>
        /// <summary>
        /// 플레이어 쪽에서 관리하는게 좋아보임
        /// 플레이어쪽 트리거감지에서 내부 감지된 콜라이더중 IPullable감지 성공 시 제어권 획득 방향
        /// </summary>
        /// 예시코드)
        /*
        [Header("플레이어의 손")]
        [SerializeField] public Transform playerHand;
        private IPullable _pullTarget;
        public bool isGrabbing = false;
        private void OnTriggerEnter2D(Collider2D other)
        {
            var pullable = other.GetComponent<IPullable>();
            if (pullable != null)
            {
                _pullTarget = pullable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_pullTarget != null && !isGrabbing)
            {
                _pullTarget = null;
            }
        }
    
        private void Update()
        {
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                 if (_pullTarget != null)
                {
                    isGrabbing = !isGrabbing;

                    if (isGrabbing)     _pullTarget.OnPull(playerHand);
                    else                _pullTarget.OnStopP();
                }
            }
        }
        */

        [Header("맵상의 장애물 오브젝트의 스폰 위치")] 
        [SerializeField] private Vector2 _spawnPos;

        [Header("오브젝트 재생성 대기시간 설정")] 
        [SerializeField] private float _respawnTime;

        [Header("플레이어와 장애물 간의 상호작용 가능 거리")] 
        [SerializeField] private float _linkDist = 1f;

        [Header("장애물이 플레이어에게 붙어 있을 거리")]
        [SerializeField] private Vector2 _pivot;

        private Rigidbody2D _rb;
        private SpriteRenderer _renderer;
        private Collider2D[] _collider;
        private Transform _playerHand;
        private bool _isPulling = false;
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
                float followTarget = _playerHand.position.x + _pivot.x;
                _rb.MovePosition(new Vector2(followTarget, _rb.position.y));

                if (transform.position.y < _spawnPos.y + 0.5f)     _rb.gravityScale = _fallingGravity;
                else                                        _rb.gravityScale = _originalGravity;
            }
        }


        public void Init()
        {
            _spawnPos = gameObject.transform.position;
            _rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponents<Collider2D>();
            _originalGravity = _rb.gravityScale;
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
                var player = _playerHand.GetComponent<PlayerController>();
                if (player != null) player.OffGrab();
            }

            _isPulling = false;
            _playerHand = null;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }

        public void Reverse()
        {

        }

        // 장애물이 사라지는 조건 (맵 밖으로 밀려남)에 위치에 장애물이 걸리게 되면 해당 메서드를 실행하면 됨
        public void Respawn()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            _rb.gravityScale = 0;
            _renderer.enabled = false;
            _collider[0].enabled = false;
            _collider[1].enabled = false;

            yield return YieldContainer.WaitForSeconds(_respawnTime);

            transform.position = _spawnPos;

            _rb.gravityScale = _originalGravity;
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
