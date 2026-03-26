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
    ///         1-1. 올라오는 속도를 조절해서 해결
    ///         2. 장애물이 솟아 오를때 장애물에 크기가 플렛폼 보다 크다면
    ///            아래쪽에 장애물이 뚫려보인다
    ///         2-1. 장애물의 y포즈가 점점 커지는 느낌으로 변경
    ///         3. 장애물이 리스폰 되는 도중 리스폰 키 입력 시 솟아오리지
    ///            않고 즉시 리스폰 되어 버리는 현상 발생
    ///         3-1. bool 변수 추가로 재생성 도중 리스폰키 추가 입력 방지
    ///         4. 반전영역에서의 장에물 오브젝트 생성 및 리스폰시
    ///            반전 영역에 맞게 재설정 필요
    ///         4-1. 반전영역에 생성될 불 변수 추가로 반전세계에 기본 스폰 관리 및 reverse/ respawn코드 수정
    ///
    /// 슬라임의 크기에 따른 대응? 추가 필요
    /// </summary>
    // 추가적으로 추상클래스에 대해 열매의 추가 기능에 따라 개편 필요
    public class Obstacle : BaseInteractionObject, IReversable
    {
        [Header("오브젝트 재생성 대기시간 설정")] 
        [SerializeField] private float _respawnTime;

        [Header("플레이어와 장애물 간의 상호작용 가능 거리")] 
        [SerializeField] private float _linkDist = 0.5f;

        [Header("장애물이 플레이어에게 붙어 있을 거리")]
        [SerializeField] private Vector2 _pivot;

        [Header("반전 세계에 있는 오브젝트")] 
        [SerializeField] private bool _isThisObjBelongsToTheReverseWorld = false;
        
        [Header("서로 반전 될 오브젝트")]
        [SerializeField] private ObstacleReverseObject _reverseObject;
        
        [Header("장애물이 솟아오르기까지 걸리는 시간")] 
        [SerializeField] private float _riseT;

        [Header("바닥 레이어 체크")] 
        [SerializeField] private LayerMask _groundLayer;
        
        [Header("바닥으로 감지될 거리")]
        [SerializeField] private float _groundDistance;
        
        [Header("바닥을 감지 할 박스의 x축 크기")]
        [SerializeField] private float _groundSizeX = 0.9f;
        
        private SpriteRenderer _renderer;
        private Collider2D _collider; 
        private float _originalGravity;
        private bool _isRespawning = false;
        private bool _isReversing = false;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            if (_isPulling && _playerHand != null)
            {
                if (_isReversing) return;
                
                Vector2 grabPoint = _collider.ClosestPoint(_playerHand.position);
                float dist = Vector2.Distance(grabPoint, _playerHand.position);

                if (dist > _linkDist)
                {
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
                float halfW = _renderer.bounds.extents.x;
                float followTarget = _playerHand.position.x + ((halfW + _pivot.x) * direction);
                
                _rb.MovePosition(new Vector2(followTarget, _rb.position.y));
                
                if (!_isReversing && !IsGrounded()) OnStopP();
            }
        }


        private void Init()
        {
            base.Init();
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _originalGravity = _rb.gravityScale;

            if (_isThisObjBelongsToTheReverseWorld) ReverseState();
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
            if (!_reverseObject.canReverse || !_isPulling) return; //당겨지고 있지 않으면 리버스가 안되도록 함

            _isReversing = true;
            
            transform.position *= new Vector2(1f, -1f);
            transform.localScale *= new Vector2(1f, -1f);

            _rb.gravityScale *= -1f;
           
            StartCoroutine(ReverseDorpObjIgnoreRoutine());
        }

        public void ReverseState()
        {
            _rb.gravityScale = -1f;
            _originalGravity = _rb.gravityScale;
            
            Vector3 scale = transform.localScale;
            scale.y *= -1f;
            transform.localScale = scale;
        }

        // 장애물이 사라지는 조건 (맵 밖으로 밀려남)에 위치에 장애물이 걸리게 되면 해당 메서드를 실행하면 됨
        public override void Respawn()
        {
            if (_isRespawning) return;
            OnStopP();
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            _isRespawning = true;
            _rb.simulated = false;
            _renderer.enabled = false;
            _collider.enabled = false;

            float direction = Mathf.Sign(_originalGravity);
            float originalScale = _isThisObjBelongsToTheReverseWorld ? 
                                  -Mathf.Abs(transform.localScale.y) : Mathf.Abs(transform.localScale.y);
            Vector2 targetScale = new Vector2(transform.localScale.x, originalScale);
            
            float startH = Mathf.Abs(originalScale) * 0.5f;
            
            Vector2 startPos = _spawnPos + Vector2.down * direction * (startH + 0.2f);
            
            transform.position = startPos;
            transform.localScale = new Vector2(targetScale.x, 0);

            yield return YieldContainer.WaitForSeconds(_respawnTime);
            
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.simulated = true;
            _renderer.enabled = true;
            _collider.enabled = true;

            float elapseTime = 0f;

            while (elapseTime < _riseT)
            {
                elapseTime += Time.deltaTime;
                float time = elapseTime / _riseT;
        
                transform.localScale = new Vector2(targetScale.x, Mathf.Lerp(0f, targetScale.y, time));
                transform.position = Vector2.Lerp(startPos, _spawnPos, time);
        
                yield return null;
            }

            transform.localScale = targetScale;
            transform.position = _spawnPos;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = _originalGravity;
            _isRespawning = false;
        }

        private IEnumerator ReverseDorpObjIgnoreRoutine()
        {
            yield return YieldContainer.WaitForSeconds(1f);
            _isReversing = false;
        }

        private bool IsGrounded()
        {
            float direction = Mathf.Sign(_rb.gravityScale);
            float checkY = (direction > 0) ? _collider.bounds.min.y : _collider.bounds.max.y;
            
            Vector2 origin = new Vector2(_collider.bounds.center.x, checkY);
            Vector2 checkBoxSize = new Vector2(_collider.bounds.size.x * _groundSizeX, 0.1f);
            
            RaycastHit2D hit =
                                Physics2D.BoxCast
                                 (
                                     origin,
                                     checkBoxSize,
                                     0f,
                                     Vector2.down * Mathf.Sign(_rb.gravityScale),
                                     _groundDistance, 
                                     _groundLayer
                                 );
            
            return hit.collider != null;
        }

        private void OnDrawGizmos()
        {
            if(_collider == null) return;
            
            float direction = Mathf.Sign(_rb.gravityScale);
            float checkY = (direction > 0) ? _collider.bounds.min.y : _collider.bounds.max.y;
            
            Vector2 origin = new Vector2(_collider.bounds.center.x, checkY);
            Vector2 checkBoxSize = new Vector2(_collider.bounds.size.x * _groundSizeX, 0.2f);
            
            Vector2 endPos = origin + (Vector2.down * direction * _groundDistance);
            
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireCube(endPos, checkBoxSize);
            Gizmos.DrawLine(origin, endPos);
            
            /*
            if (_playerHand != null)
            {
                float dist = Vector2.Distance(_collider.ClosestPoint(_playerHand.position), _playerHand.position);
        
                if (dist > _linkDist) Gizmos.color = Color.red;
                else                  Gizmos.color = Color.green;
        
                Gizmos.DrawLine(transform.position, _playerHand.position);
                Vector3 cubeSize = new Vector3(_linkDist * 2, _linkDist * 2, 0.1f);
        
                if (_isPulling && _playerHand != null && _renderer != null)
                {
                    float direction = (_playerHand.position.x > transform.position.x) ? -1f : 1f;
                    float halfW = _renderer.bounds.extents.x;
                    float followTarget = _playerHand.position.x + ((halfW + _pivot.x) * direction);
                    Vector3 targetPos = new Vector3(followTarget, transform.position.y, 0);
        
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(targetPos, cubeSize);
                }
            }
            */
        }
    }
}
