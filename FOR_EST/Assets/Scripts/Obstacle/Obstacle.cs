using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Obstacle
{
    public class Obstacle : BaseInteractionObject, IReversable
    {
        [Header("장애물이 플레이어에게 붙어 있을 거리")]
        [SerializeField] private Vector2 _pivot;

        [Header("반전 세계에 있는 오브젝트")] 
        [SerializeField] public bool _isThisObjBelongsToTheReverseWorld = false;

        [Header("서로 반전 될 오브젝트")] 
        [SerializeField] private GameObject _reverseObjectPrefab;
        
        [Header("장애물이 솟아오르기까지 걸리는 시간")] 
        [SerializeField] private float _riseT;

        [Header("바닥만 레이어 체크")] 
        [SerializeField] private LayerMask _groundLayer;

        [Header("반전 불가능한 전체 레이어 체크")] 
        [SerializeField] private LayerMask _groundReverseLayer;
        
        [Header("바닥으로 감지될 거리")]
        [SerializeField] private float _groundDistance;
        
        [Header("바닥을 감지 할 박스의 x축 크기")]
        [SerializeField] private float _groundSizeX = 0.9f;
        
        private ObstacleReverseObject _reverseObjectScript;
        private float _originalGravity;
        private bool _isReversing = false;
        public bool _isReverse = false;

        private void Awake()
        {
            Init();
        }

        public override void Update()
        {
            if (!_isReversing) base.Update();

            // 테스트 코드 후에 리스폰 조건 생성시 삭제해야함 
            if (Keyboard.current.rKey.wasPressedThisFrame) base.Respawn();
        }

        private void FixedUpdate()
        {
            if (_isPulling && _playerHand != null)
            {
                float direction = (_playerHand.position.x > transform.position.x) ? -1f : 1f;
                float halfW = _renderer.bounds.extents.x;
                float followTarget = _playerHand.position.x + ((halfW + _pivot.x) * direction);
                
                _rb.MovePosition(new Vector2(followTarget, _rb.position.y));
                
                if (!_isReversing && !IsGrounded()) base.OnStopP();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Boundary"))
            {
                base.Respawn();
            }
        }

        private void Init()
        {
            base.Init();
            _originalGravity = _rb.gravityScale;
            if (_isThisObjBelongsToTheReverseWorld) ReversingState();
            if (_reverseObjectPrefab != null) _reverseObjectPrefab = Instantiate(_reverseObjectPrefab);
            _reverseObjectScript = _reverseObjectPrefab.GetComponent<ObstacleReverseObject>();
            if(_reverseObjectScript != null) _reverseObjectScript.Init(this.gameObject, this);

        }

        public void Reverse()
        {
            if (_reverseObjectScript == null) Init();
            if (!_reverseObjectScript.canReverse) return;
            _reverseObjectScript.OnReverseGround();
            if (!_reverseObjectScript.canReverse || !_isPulling || !_reverseObjectScript.OnGround) return; //당겨지고 있지 않으면 리버스가 안되도록 함

            _isReversing = true;
            _isReverse = !_isReverse;
            
            transform.position *= new Vector2(1f, -1f);
            transform.localScale *= new Vector2(1f, -1f);

            _rb.gravityScale *= -1f;
           
            StartCoroutine(ReverseDorpObjIgnoreRoutine());
        }

        public void ReversingState()
        {
            _rb.gravityScale *= -1f;
            _originalGravity = _rb.gravityScale;
            _isReverse = true;
            
            Vector3 scale = transform.localScale;
            scale.y *= -1f;
            transform.localScale = scale;
        }


        public override IEnumerator RespawnRoutine()
        {
            _isRespawning = true;
            base.RespawningState(false);

            float direction = Mathf.Sign(_originalGravity);
            float originalScale = _isThisObjBelongsToTheReverseWorld ? 
                                  -Mathf.Abs(transform.localScale.y) : Mathf.Abs(transform.localScale.y);
            float startH = Mathf.Abs(originalScale) * 0.5f;
            
            Vector2 targetScale = new Vector2(transform.localScale.x, originalScale);
            Vector2 startPos = _spawnPos + Vector2.down * direction * (startH + 0.2f);
            
            transform.position = startPos;
            transform.localScale = new Vector2(targetScale.x, 0);

            yield return YieldContainer.WaitForSeconds(_respawnTime);
            
            _rb.bodyType = RigidbodyType2D.Kinematic;
            base.RespawningState(true);

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
            base.PullingState(false);
        }

        private IEnumerator ReverseDorpObjIgnoreRoutine()
        {
            yield return YieldContainer.WaitForSeconds(0.1f);
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
