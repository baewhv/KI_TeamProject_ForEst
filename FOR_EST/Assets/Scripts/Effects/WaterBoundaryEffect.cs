using UnityEngine;

/// <summary>
/// 슬픔/행복 세계를 나누는 경계선(X축, Y=0) 충돌 이펙트.
/// - 위쪽(슬픔 세상): 빗방울이 수면에 닿아 물이 위로 튀어 오르는 효과
/// - 아래쪽(행복 세상): 물속으로 들어갈 때처럼 기포가 올라오는 효과
///
/// [사용법]
/// 1. 빈 오브젝트에 이 컴포넌트를 추가합니다.
/// 2. _boundaryY 를 경계선 Y 좌표에 맞게 설정합니다 (기본: 0).
/// 3. _halfWidth 를 화면 가로 범위에 맞게 설정합니다.
/// 4. SadRain/HappyRain 의 _spawnRate 와 유사하게 각 Rate 값을 조절합니다.
/// </summary>
public class WaterBoundaryEffect : MonoBehaviour
{
    // ────────────────────────────────────────────────
    //  공통 설정
    // ────────────────────────────────────────────────
    [Header("공통 설정")]
    [Tooltip("경계선 Y 위치 (슬픔/행복 세계 경계)")]
    [SerializeField] private float _boundaryY = 0f;

    [Tooltip("이펙트가 생성될 X축 범위 (중심 기준 ±halfWidth)")]
    [SerializeField] private float _halfWidth = 10f;

    // ────────────────────────────────────────────────
    //  위쪽 스플래시 설정 (경계 위, 슬픔 세상 쪽)
    // ────────────────────────────────────────────────
    [Header("위쪽 스플래시 설정 (경계 위)")]
    [Tooltip("초당 스플래시 생성 횟수. SadRain의 _spawnRate와 유사하게 설정")]
    [SerializeField] private float _splashRate = 18f;

    [Tooltip("스플래시 위쪽 초기 속도 최솟값 (유닛/초)")]
    [SerializeField] private float _splashSpeedMin = 1.5f;

    [Tooltip("스플래시 위쪽 초기 속도 최댓값 (유닛/초)")]
    [SerializeField] private float _splashSpeedMax = 3.5f;

    [Tooltip("스플래시 X축 초기 속도 범위 (±, 유닛/초)")]
    [SerializeField] private float _splashXSpread = 1.2f;

    [Tooltip("스플래시에 작용하는 중력값")]
    [SerializeField] private float _splashGravity = 9f;

    [Tooltip("스플래시 최대 수명 (초). 이 시간 전에 경계선 아래로 내려가면 소멸")]
    [SerializeField] private float _splashLifetime = 0.55f;

    [Tooltip("스플래시 색상")]
    [SerializeField] private Color _splashColor = new Color(0.55f, 0.75f, 1f, 0.75f);

    [Tooltip("스플래시 너비 (월드 유닛)")]
    [SerializeField] private float _splashWidth = 0.035f;

    [Tooltip("스플래시 높이 (월드 유닛)")]
    [SerializeField] private float _splashHeight = 0.1f;

    [Tooltip("스플래시 오브젝트 풀 크기 (최대 동시 개수)")]
    [SerializeField] private int _splashPoolSize = 200;

    // ────────────────────────────────────────────────
    //  아래쪽 기포 설정 (경계 아래, 행복 세상 쪽)
    // ────────────────────────────────────────────────
    [Header("아래쪽 기포 설정 (경계 아래)")]
    [Tooltip("초당 기포 생성 횟수")]
    [SerializeField] private float _bubbleRate = 12f;

    [Tooltip("기포 가라앉는 속도 (유닛/초)")]
    [SerializeField] private float _bubbleSinkSpeed = 0.6f;

    [Tooltip("기포가 좌우로 흔들리는 속도 (라디안/초)")]
    [SerializeField] private float _bubbleWobbleSpeed = 3f;

    [Tooltip("기포 좌우 흔들림 폭 (월드 유닛)")]
    [SerializeField] private float _bubbleWobbleAmount = 0.08f;

    [Tooltip("기포 수명 (초). 이 시간 동안 서서히 가라앉으며 투명해짐")]
    [SerializeField] private float _bubbleLifetime = 1.0f;

    [Tooltip("기포 생성 Y 오프셋 범위 (경계선 바로 아래)")]
    [SerializeField] private float _bubbleSpawnDepth = 0.15f;

    [Tooltip("기포 색상")]
    [SerializeField] private Color _bubbleColor = new Color(0.75f, 0.92f, 1f, 0.65f);

    [Tooltip("기포 크기 최솟값 (월드 유닛)")]
    [SerializeField] private float _bubbleSizeMin = 0.04f;

    [Tooltip("기포 크기 최댓값 (월드 유닛)")]
    [SerializeField] private float _bubbleSizeMax = 0.13f;

    [Tooltip("기포 오브젝트 풀 크기 (최대 동시 개수)")]
    [SerializeField] private int _bubblePoolSize = 120;

    // ────────────────────────────────────────────────
    //  렌더링
    // ────────────────────────────────────────────────
    [Header("렌더링")]
    [SerializeField] private string _sortingLayerName = "Default";
    [SerializeField] private int _sortingOrder = 11;

    // ────────────────────────────────────────────────
    //  내부 구조체
    // ────────────────────────────────────────────────
    private struct SplashDrop
    {
        public Transform     tr;
        public SpriteRenderer sr;
        public float x, y;
        public float vx, vy;
        public float age;
        public float maxAge;
    }

    private struct Bubble
    {
        public Transform      tr;
        public SpriteRenderer sr;
        public float x, y;
        public float wobbleOffset;
        public float age;
        public float maxAge;
    }

    // ────────────────────────────────────────────────
    //  내부 상태
    // ────────────────────────────────────────────────
    private SplashDrop[] _splashPool;
    private Bubble[]     _bubblePool;
    private float        _splashTimer;
    private float        _bubbleTimer;

    private static Sprite   _rectSprite;
    private static Sprite   _circleSprite;
    private static Material _mat;

    // ────────────────────────────────────────────────
    //  Unity 생명주기
    // ────────────────────────────────────────────────
    private void Awake()
    {
        _splashPool = new SplashDrop[_splashPoolSize];
        _bubblePool = new Bubble[_bubblePoolSize];

        if (_rectSprite   == null) _rectSprite   = CreateRectSprite();
        if (_circleSprite == null) _circleSprite = CreateCircleSprite();
        if (_mat          == null) _mat = new Material(Shader.Find("Sprites/Default"));
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 스폰 타이머
        _splashTimer += dt;
        float splashInterval = 1f / Mathf.Max(_splashRate, 0.001f);
        while (_splashTimer >= splashInterval)
        {
            _splashTimer -= splashInterval;
            TrySpawnSplash();
        }

        _bubbleTimer += dt;
        float bubbleInterval = 1f / Mathf.Max(_bubbleRate, 0.001f);
        while (_bubbleTimer >= bubbleInterval)
        {
            _bubbleTimer -= bubbleInterval;
            TrySpawnBubble();
        }

        UpdateSplashes(dt);
        UpdateBubbles(dt);
    }

    // ────────────────────────────────────────────────
    //  업데이트: 스플래시
    // ────────────────────────────────────────────────
    private void UpdateSplashes(float dt)
    {
        for (int i = 0; i < _splashPoolSize; i++)
        {
            ref SplashDrop s = ref _splashPool[i];
            if (s.tr == null) continue;

            s.age += dt;
            float t = s.age / s.maxAge;

            // 수명 초과 또는 경계선 아래로 내려가면 소멸
            if (t >= 1f || s.y < _boundaryY - 0.05f)
            {
                Destroy(s.tr.gameObject);
                s.tr = null;
                continue;
            }

            // 중력 적용 후 이동
            s.vy -= _splashGravity * dt;
            s.x  += s.vx * dt;
            s.y  += s.vy * dt;
            s.tr.position = new Vector3(s.x, s.y, 0f);

            // 위로 올라갈수록 높이가 줄어드는 효과 (위에서 아래로 압축)
            float heightScale = Mathf.Lerp(_splashHeight, _splashHeight * 0.4f, t);
            s.tr.localScale = new Vector3(_splashWidth, heightScale, 1f);

            // 페이드 아웃 (t^2 로 자연스럽게)
            Color c = _splashColor;
            c.a = _splashColor.a * (1f - t * t);
            s.sr.color = c;
        }
    }

    // ────────────────────────────────────────────────
    //  업데이트: 기포
    // ────────────────────────────────────────────────
    private void UpdateBubbles(float dt)
    {
        for (int i = 0; i < _bubblePoolSize; i++)
        {
            ref Bubble b = ref _bubblePool[i];
            if (b.tr == null) continue;

            b.age += dt;
            float t = b.age / b.maxAge;

            // 수명 초과 시 소멸
            if (t >= 1f)
            {
                Destroy(b.tr.gameObject);
                b.tr = null;
                continue;
            }

            // 아래로 가라앉기 + 좌우 흔들림 (점점 줄어듦)
            b.y -= _bubbleSinkSpeed * dt;
            float wobble = _bubbleWobbleAmount * (1f - t); // 가라앉을수록 흔들림 감소
            float wobbleX = Mathf.Sin(b.wobbleOffset + b.age * _bubbleWobbleSpeed) * wobble;
            b.tr.position = new Vector3(b.x + wobbleX, b.y, 0f);

            // 전체 수명에 걸쳐 서서히 페이드 아웃
            Color c = _bubbleColor;
            c.a = _bubbleColor.a * (1f - t);
            b.sr.color = c;
        }
    }

    // ────────────────────────────────────────────────
    //  스폰: 스플래시
    // ────────────────────────────────────────────────
    private void TrySpawnSplash()
    {
        for (int i = 0; i < _splashPoolSize; i++)
        {
            if (_splashPool[i].tr != null) continue;

            float x = transform.position.x + Random.Range(-_halfWidth, _halfWidth);

            var go = new GameObject("BoundarySplash");
            go.transform.SetParent(transform, false);
            go.transform.position   = new Vector3(x, _boundaryY, 0f);
            go.transform.localScale = new Vector3(_splashWidth, _splashHeight, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite           = _rectSprite;
            sr.material         = _mat;
            sr.color            = _splashColor;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder     = _sortingOrder;

            _splashPool[i] = new SplashDrop
            {
                tr     = go.transform,
                sr     = sr,
                x      = x,
                y      = _boundaryY,
                vx     = Random.Range(-_splashXSpread, _splashXSpread),
                vy     = Random.Range(_splashSpeedMin, _splashSpeedMax),
                age    = 0f,
                maxAge = _splashLifetime
            };
            return;
        }
    }

    // ────────────────────────────────────────────────
    //  스폰: 기포
    // ────────────────────────────────────────────────
    private void TrySpawnBubble()
    {
        for (int i = 0; i < _bubblePoolSize; i++)
        {
            if (_bubblePool[i].tr != null) continue;

            float x    = transform.position.x + Random.Range(-_halfWidth, _halfWidth);
            float y    = _boundaryY - Random.Range(0f, _bubbleSpawnDepth); // 경계선 바로 아래에 생성
            float size = Random.Range(_bubbleSizeMin, _bubbleSizeMax);

            var go = new GameObject("BoundaryBubble");
            go.transform.SetParent(transform, false);
            go.transform.position   = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(size, size, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite           = _circleSprite;
            sr.material         = _mat;
            sr.color            = _bubbleColor;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder     = _sortingOrder;

            _bubblePool[i] = new Bubble
            {
                tr           = go.transform,
                sr           = sr,
                x            = x,
                y            = y,
                wobbleOffset = Random.Range(0f, Mathf.PI * 2f),
                age          = 0f,
                maxAge       = _bubbleLifetime * Random.Range(0.7f, 1.3f)
            };
            return;
        }
    }

    // ────────────────────────────────────────────────
    //  스프라이트 생성 유틸리티
    // ────────────────────────────────────────────────

    // 직사각형 스프라이트 (스플래시용)
    private static Sprite CreateRectSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    // 원형 테두리 스프라이트 (기포용)
    private static Sprite CreateCircleSprite()
    {
        const int res = 32;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float center      = (res - 1) * 0.5f;
        float outerRadius = center * 0.92f;
        float ringWidth   = center * 0.28f;    // 테두리 두께
        float innerRadius = outerRadius - ringWidth;

        for (int py = 0; py < res; py++)
        {
            for (int px = 0; px < res; px++)
            {
                float dist = Mathf.Sqrt((px - center) * (px - center) + (py - center) * (py - center));

                float alpha = 0f;
                if (dist <= outerRadius && dist >= innerRadius)
                {
                    // 테두리 안쪽/바깥쪽으로 갈수록 부드럽게 페이드
                    float edgeOuter = Mathf.Clamp01((outerRadius - dist) / (ringWidth * 0.35f));
                    float edgeInner = Mathf.Clamp01((dist - innerRadius) / (ringWidth * 0.35f));
                    alpha = Mathf.Min(edgeOuter, edgeInner);
                }
                else if (dist < innerRadius)
                {
                    // 기포 내부: 아주 옅게 채움 (기포 속 공기 느낌)
                    float inner = Mathf.Clamp01(1f - dist / innerRadius);
                    alpha = inner * 0.12f;
                }

                tex.SetPixel(px, py, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    // ────────────────────────────────────────────────
    //  Gizmos
    // ────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        float cx = transform.position.x;

        // 경계선 (흰색)
        Gizmos.color = new Color(1f, 1f, 1f, 0.9f);
        Gizmos.DrawLine(new Vector3(cx - _halfWidth, _boundaryY, 0f),
                        new Vector3(cx + _halfWidth, _boundaryY, 0f));

        // 스플래시 최대 도달 높이 (파란색 점선 느낌)
        float maxSplashY = _boundaryY + _splashSpeedMax * _splashSpeedMax / (2f * _splashGravity);
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.5f);
        Gizmos.DrawLine(new Vector3(cx - _halfWidth, maxSplashY, 0f),
                        new Vector3(cx + _halfWidth, maxSplashY, 0f));

        // 기포 스폰 범위 + 가라앉는 최대 거리 (초록색)
        float sinkDist = _bubbleSinkSpeed * _bubbleLifetime;
        Gizmos.color = new Color(0.4f, 1f, 0.7f, 0.5f);
        Gizmos.DrawLine(new Vector3(cx - _halfWidth, _boundaryY - _bubbleSpawnDepth, 0f),
                        new Vector3(cx + _halfWidth, _boundaryY - _bubbleSpawnDepth, 0f));
        Gizmos.color = new Color(0.4f, 1f, 0.7f, 0.25f);
        Gizmos.DrawLine(new Vector3(cx - _halfWidth, _boundaryY - _bubbleSpawnDepth - sinkDist, 0f),
                        new Vector3(cx + _halfWidth, _boundaryY - _bubbleSpawnDepth - sinkDist, 0f));
    }
}
