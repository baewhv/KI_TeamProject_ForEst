using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Obstacle
{
    public class Obstacle : BaseInteractionObject, IReversable
    {
        /// <summary>
        /// 열매와의 공통 기능
        ///         1. 콜라이더와 렌더러 조정
        ///         2. onpull 전체
        ///         3. onstopp 전체
        ///         4. rigidbody의 freeze기능 사용부분
        ///         5. respawn코루틴
        ///                 차이? 열매는 즉시 생성 / 장애물은 기획의도상 솟아오름
        /// 현재 상태에서 지원님 작업이 끝나면 추가적인 부분 확인 후 열매쪽에도 공통부분은 베이스로 업 
        /// 예상 추가 부분 ) 업데이트쪽 플레이어와의 연결부 코드 구현 해주시는 방식에 따라 해당 부분도 오버라이딩 시킬 수 있을 것으로 예상
        /// </summary>
        [Header("플레이어와 장애물 간의 상호작용 가능 거리")] 
        [SerializeField] private float _linkDist = 0.5f;

        [Header("장애물이 플레이어에게 붙어 있을 거리")]
        [SerializeField] private Vector2 _pivot;

        [Header("반전 세계에 있는 오브젝트")] 
        [SerializeField] private bool _isThisObjBelongsToTheReverseWorld = false;
        
        [Header("서로 반전 될 오브젝트")]
        [SerializeField] private ReverseObject _reverseObject;
        
        [Header("장애물이 솟아오르기까지 걸리는 시간")] 
        [SerializeField] private float _riseT;

        [Header("바닥 레이어 체크")] 
        [SerializeField] private LayerMask _groundLayer;
        
        [Header("바닥으로 감지될 거리")]
        [SerializeField] private float _groundDistance;
        
        [Header("바닥을 감지 할 박스의 x축 크기")]
        [SerializeField] private float _groundSizeX = 0.9f;
        
        private float _originalGravity;
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
                    base.OnStopP();
                }
            }

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


        private void Init()
        {
            base.Init();
            _originalGravity = _rb.gravityScale;

            if (_isThisObjBelongsToTheReverseWorld) ReversingState();
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

        public void ReversingState()
        {
            _rb.gravityScale = -1f;
            _originalGravity = _rb.gravityScale;
            
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
            yield return YieldContainer.WFFU;
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
