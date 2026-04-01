using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터가 지면을 걸을 때 발 뒤로 흙먼지 파티클을 생성하는 컴포넌트.
/// 외부 ParticleSystem 설정 없이 코드로 완전 동작합니다.
/// 플레이어 오브젝트 또는 그 자식 오브젝트에 붙여 사용하세요.
/// </summary>
public class WalkDustEffect : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("PlayerMovement 컴포넌트 (비워두면 부모에서 자동 탐색)")]
    [SerializeField] private PlayerMovement _playerMovement;
    [Tooltip("PlayerController 컴포넌트 (비워두면 부모에서 자동 탐색)")]
    [SerializeField] private PlayerController _playerController;

    [Header("발 위치 오프셋")]
    [Tooltip("캐릭터 중심 기준 발 Y 오프셋 (양수 입력, 방향은 중력에 따라 자동)")]
    [SerializeField] private float _footOffsetY = 0.5f;

    [Header("방출 설정")]
    [Tooltip("초당 파티클 방출 수 (걸을 때)")]
    [Range(1f, 60f)]
    [SerializeField] private float _emitRate = 18f;

    [Tooltip("이동으로 인정하는 최소 X 속도")]
    [SerializeField] private float _minMoveSpeed = 0.05f;

    [Header("파티클 크기")]
    [Tooltip("최소 파티클 크기")]
    [SerializeField] private float _minSize = 0.06f;

    [Tooltip("최대 파티클 크기")]
    [SerializeField] private float _maxSize = 0.18f;

    [Header("파티클 수명")]
    [Tooltip("파티클 최소 수명 (초)")]
    [SerializeField] private float _minLifetime = 0.20f;

    [Tooltip("파티클 최대 수명 (초)")]
    [SerializeField] private float _maxLifetime = 0.45f;

    [Header("속도 설정")]
    [Tooltip("뒤쪽 방향 초기 속도 (이동 반대 방향)")]
    [SerializeField] private float _backwardSpeed = 1.2f;

    [Tooltip("지면에서 튀어 오르는 초기 속도 (중력 반대 방향)")]
    [SerializeField] private float _upwardSpeed = 1.4f;

    [Tooltip("수평 퍼짐 랜덤 범위 (±)")]
    [SerializeField] private float _spreadX = 0.4f;

    [Tooltip("파티클에 작용하는 중력 가속도 (낙하용)")]
    [SerializeField] private float _gravity = 4.0f;

    [Header("색상 설정")]
    [Tooltip("흙먼지 색상 목록 (갈색/베이지 계열)")]
    [SerializeField] private Color[] _colors = new Color[]
    {
        new Color(0.72f, 0.58f, 0.40f), // 황토색
        new Color(0.65f, 0.52f, 0.36f), // 갈색
        new Color(0.80f, 0.68f, 0.50f), // 연한 모래색
        new Color(0.60f, 0.48f, 0.33f), // 짙은 황토
        new Color(0.75f, 0.63f, 0.46f), // 흙색
    };

    [Header("페이드 설정")]
    [Tooltip("파티클 생성 직후 최대 알파")]
    [Range(0f, 1f)]
    [SerializeField] private float _maxAlpha = 0.70f;

    [Header("렌더링")]
    [Tooltip("소팅 레이어 이름")]
    [SerializeField] private string _sortingLayerName = "Default";

    [Tooltip("소팅 오더 (캐릭터보다 낮게 설정 권장)")]
    [SerializeField] private int _sortingOrder = 1;

    [Header("풀 설정")]
    [Tooltip("파티클 오브젝트 풀 크기 (emitRate × maxLifetime 이상 권장)")]
    [SerializeField] private int _poolSize = 30;

    // ──────────────────────────────────────────────
    //  내부 파티클 데이터
    // ──────────────────────────────────────────────
    private class DustParticle
    {
        public GameObject go;
        public SpriteRenderer sr;
        public bool active;
        public float lifetime;
        public float maxLifetime;
        public Vector3 velocity;    // 현재 속도
        public float gravityDir;    // 중력 방향 (+1 위, -1 아래)
        public Color baseColor;
        public float initAlpha;
        public float initSize;
    }

    private readonly List<DustParticle> _pool = new List<DustParticle>();
    private Sprite _circleSprite;
    private float _emitTimer;

    // ──────────────────────────────────────────────
    //  초기화
    // ──────────────────────────────────────────────
    private void Awake()
    {
        if (_playerMovement == null)
            _playerMovement = GetComponentInParent<PlayerMovement>();
        if (_playerController == null)
            _playerController = GetComponentInParent<PlayerController>();

        _circleSprite = CreateCircleSprite(20);
        BuildPool();
    }

    private void BuildPool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            var go = new GameObject("DustParticle");
            go.transform.SetParent(transform);
            go.SetActive(false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _circleSprite;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder = _sortingOrder;

            _pool.Add(new DustParticle { go = go, sr = sr, active = false });
        }
    }

    // ──────────────────────────────────────────────
    //  매 프레임 업데이트
    // ──────────────────────────────────────────────
    private void Update()
    {
        if (_playerMovement == null || _playerController == null) return;

        bool isReverse = _playerController.IsReverse;
        float velX = _playerMovement._rigidbody.linearVelocityX;
        bool isMoving = Mathf.Abs(velX) > _minMoveSpeed;
        bool isGrounded = _playerMovement.IsGround();

        // 방출
        if (isMoving && isGrounded)
        {
            _emitTimer += Time.deltaTime;
            float interval = 1f / _emitRate;
            while (_emitTimer >= interval)
            {
                _emitTimer -= interval;
                EmitDust(velX, isReverse);
            }
        }
        else
        {
            _emitTimer = 0f;
        }

        // 활성 파티클 업데이트
        float dt = Time.deltaTime;
        foreach (var p in _pool)
        {
            if (!p.active) continue;

            p.lifetime += dt;

            // 수명 종료
            if (p.lifetime >= p.maxLifetime)
            {
                ReturnToPool(p);
                continue;
            }

            // 위치 이동
            p.velocity.y -= p.gravityDir * _gravity * dt;
            p.go.transform.position += p.velocity * dt;

            // 수명 진행률 (0=생성, 1=소멸)
            float t = p.lifetime / p.maxLifetime;

            // 알파 페이드 아웃 (초반 유지 → 후반 빠르게 소멸)
            float alpha = p.initAlpha * (1f - Mathf.Pow(t, 1.5f));
            p.sr.color = new Color(p.baseColor.r, p.baseColor.g, p.baseColor.b, alpha);

            // 크기 축소
            float scale = p.initSize * (1f - t * 0.5f);
            p.go.transform.localScale = Vector3.one * scale;
        }
    }

    // ──────────────────────────────────────────────
    //  파티클 방출
    // ──────────────────────────────────────────────
    private void EmitDust(float velX, bool isReverse)
    {
        DustParticle p = GetFromPool();
        if (p == null) return;

        // 발 위치 계산
        float footSign = isReverse ? 1f : -1f;
        Vector3 footPos = transform.position + new Vector3(0f, _footOffsetY * footSign, 0f);
        p.go.transform.position = footPos;

        // 초기 속도: 이동 반대 방향 + 지면에서 위로 튀는 방향
        float moveDir = velX > 0f ? 1f : -1f;
        float backDir = -moveDir;
        float upSign = isReverse ? -1f : 1f; // 반전 시 "위"는 아래

        float vx = backDir * _backwardSpeed + Random.Range(-_spreadX, _spreadX);
        float vy = upSign * _upwardSpeed * Random.Range(0.5f, 1.0f);
        p.velocity = new Vector3(vx, vy, 0f);

        // 중력 방향 (파티클을 다시 지면 쪽으로 당김)
        // 일반: 중력이 아래(-y)이므로 gravityDir = 1(아래)
        // 반전: 중력이 위(+y)이므로 gravityDir = -1(위)
        p.gravityDir = isReverse ? -1f : 1f;

        // 시각 초기화
        float size = Random.Range(_minSize, _maxSize);
        Color col = _colors.Length > 0 ? _colors[Random.Range(0, _colors.Length)] : Color.gray;
        float alpha = _maxAlpha * Random.Range(0.5f, 1.0f);

        p.baseColor = col;
        p.initAlpha = alpha;
        p.initSize = size;
        p.maxLifetime = Random.Range(_minLifetime, _maxLifetime);
        p.lifetime = 0f;

        p.go.transform.localScale = Vector3.one * size;
        p.sr.color = new Color(col.r, col.g, col.b, alpha);
        p.go.SetActive(true);
        p.active = true;
    }

    // ──────────────────────────────────────────────
    //  오브젝트 풀
    // ──────────────────────────────────────────────
    private DustParticle GetFromPool()
    {
        foreach (var p in _pool)
        {
            if (!p.active) return p;
        }
        return null; // 풀 소진 시 스킵 (크기 늘리려면 _poolSize 증가)
    }

    private void ReturnToPool(DustParticle p)
    {
        p.active = false;
        p.go.SetActive(false);
    }

    // ──────────────────────────────────────────────
    //  원형(소프트) 스프라이트 동적 생성
    // ──────────────────────────────────────────────
    private static Sprite CreateCircleSprite(int resolution)
    {
        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = resolution * 0.5f;
        float r = center;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                float t = dist / r;
                // 흙먼지: 중심은 뭉툭하게, 가장자리는 부드럽게
                float alpha = dist < r ? Mathf.Pow(Mathf.Max(0f, 1f - t * t), 1.2f) : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    // ──────────────────────────────────────────────
    //  에디터 기즈모
    // ──────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        bool isReverse = _playerController != null && _playerController.IsReverse;
        float footSign = isReverse ? 1f : -1f;
        Vector3 footPos = transform.position + new Vector3(0f, _footOffsetY * footSign, 0f);

        Gizmos.color = new Color(0.8f, 0.6f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(footPos, 0.1f);

        // 방출 범위 표시
        Gizmos.color = new Color(0.8f, 0.6f, 0.2f, 0.3f);
        Gizmos.DrawWireCube(footPos, new Vector3(_spreadX * 2f, 0.05f, 0f));
    }
}
