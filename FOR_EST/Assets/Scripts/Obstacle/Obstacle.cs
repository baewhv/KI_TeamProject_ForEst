using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Obstacle
{
    public class Obstacle : MonoBehaviour, IPullable, IReversable
    {
        /// <summary>
        /// 상호작용한 물체가 플레이어의 손이면 손의 하위 오브젝트로 장애물을 종속 시키기 위함
        /// 문제점 1.  플레이어의 하위 오브젝트로 종속된다면 플레이어의 중력 영향을 같이 받아오는지 실험 필요 (중력 영향은 따로 받음)
        ///       2.  플레이어의 하단엔 발판이 있고 장애물의 하단에 발판이 없을 경우 즉시 아래로 떨어져야 함? (이야기가 나왔던 부분 같은데 까먹음)
        ///           (플레이어에게 매달린 상태에선 y좌표 값을 고정시켰기에 떨어지지 않음 만약 떨어지지 않아야 할 경우 개선 필요)
        ///           (개선방안의 경우 그냥 y포지션 프리즈를 풀어버리면 됨 / 단 이경우 계단을 오르는 경우에 들고 점프가 가능한지 확인 필요 )
        ///       2.5. 만약 계단식 지형이 있고 플레이어의 앞칸 계단에 장애물이 놓여 있을 경우 + 허공의 플렛폼 끝자락에 위치 할 경우 
        ///           (플레이어가 장애물을 놓침 vs 잡은 상태로 허공에 장애물이 유지 됨)
        ///            또는 프리즈를 풀어버린 경우 계단식 지형을 오를 때 장애물이 같이 튀어오르지 않을 가능성이 높다고 보고있음
        ///       3.  플레이어가 점프 할 경우 이는 기획팀의 기획 방향에 맞춰서 변경 필요 가능할 경우를 상정해 적용
        ///           (현재 상황에선 플레이어가 점프시 같이 뛰어오를 것으로 보임
        ///           계속 연결된 상태로 플레이어만 위아래로 왔다갔다 할지 아예 점프 불가능할지 점프 시 블록을 놓치게 될지는 나도 몰루)
        ///       4.  만약 플레이어의 하위 오브젝트로 장애물을 종속 시킬 경우에 생길 추가적인 문제점들 파악 필요 
        /// 손의 위치가 어딘지에 상관없이 키를 누르면 붙게 됨
        ///     해결방안. 3번째 콜라이더를 만들어 해당 콜라이더 내부에 플레이어의 손이 들어올 경우 작동하게 만든다
        ///
        /// 추가 필요
        ///     반전 기능
        ///         플레이어의 하위에 종속될 경우 별도의 기능 추가 없이 플레이어와 함께 반전 될 가능성
        ///         그럼 플레이어의 자식오브젝트 상태에선 플레이어 감지 콜라이더를 임시 비활 처리를 통해 너무 큰 범위에 벽이 감지되는걸 방지?
        ///     스폰위치 저장 및 특정 상황 발생시(플레이어가 오브젝트 위치 재설정을 위한 임의의 파괴 시)
        ///         오브젝트 파괴 이펙트 후 저장 해둔 스폰위치로 오브젝트 재생성 기능
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
        private bool isGrabbing = false;
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
            if (Keyboard.current.anyKey.wasPressedThisFrame)
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
        
        private Rigidbody2D _rb;
        private SpriteRenderer _renderer;
        private Collider2D[] _collider;
        
        
    
        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _spawnPos = gameObject.transform.position;
            _rb       = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponents<Collider2D>();
        }

        public void OnPull(Transform playerHand)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezePosition;
            gameObject.transform.SetParent(playerHand.gameObject.transform);
        }

        public void OnStopP()
        {
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            gameObject.transform.SetParent(null);
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
            _renderer.enabled = false;
            _collider[0].enabled = false;
            _collider[1].enabled = false;
            
            yield return YieldContainer.WaitForSeconds(_respawnTime);
            
            transform.position = _spawnPos;
            
            _renderer.enabled = true;
            _collider[0].enabled = true;
            _collider[1].enabled = true;
        }
    }
}
