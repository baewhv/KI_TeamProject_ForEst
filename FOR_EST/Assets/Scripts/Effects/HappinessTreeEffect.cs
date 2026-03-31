using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 행복나무에서 밝은 기운과 나뭇잎 파티클이 생성되어 x축(y=0)까지 떠오르는 이펙트.
/// 행복나무 오브젝트 또는 대응하는 빈 오브젝트에 붙여 사용하세요.
/// transform.position을 나무 뿌리 기준점(x축에 가까운 쪽)으로 맞추고 깊이 오프셋을 조정하세요.
/// </summary>
public class HappinessTreeEffect : MonoBehaviour
{
    [Header("파티클 생성 설정")]
    [Tooltip("동시에 존재하는 잎/빛 파티클 수")]
    [Range(10, 300)]
    [SerializeField] private int _count = 80;

    [Header("나무 영역 설정")]
    [Tooltip("파티클 스폰 X 범위 (transform 중심 기준 ±)")]
    [SerializeField] private float _spawnHalfWidth = 3.0f;

    [Tooltip("스폰 최소 깊이 오프셋 (transform.y 기준 아래로, 나무 윗부분)")]
    [SerializeField] private float _spawnDepthMin = 0.5f;

    [Tooltip("스폰 최대 깊이 오프셋 (transform.y 기준 아래로, 나무 아랫부분)")]
    [SerializeField] private float _spawnDepthMax = 5.5f;

    [Header("상승 목표")]
    [Tooltip("상승 목표 Y (x축 위치, 기본 0)")]
    [SerializeField] private float _groundY = 0f;

    [Header("파티클 크기")]
    [Tooltip("최소 파티클 크기")]
    [SerializeField] private float _minSize = 0.04f;

    [Tooltip("최대 파티클 크기")]
    [SerializeField] private float _maxSize = 0.16f;

    [Header("상승 속도")]
    [Tooltip("최소 상승 속도 (유닛/초)")]
    [SerializeField] private float _minRiseSpeed = 0.4f;

    [Tooltip("최대 상승 속도 (유닛/초)")]
    [SerializeField] private float _maxRiseSpeed = 1.2f;

    [Header("흔들림 설정")]
    [Tooltip("수평 흔들림 강도 (잎이 바람에 흔들리는 정도)")]
    [SerializeField] private float _wobbleAmplitude = 0.30f;

    [Tooltip("흔들림 속도")]
    [SerializeField] private float _wobbleSpeed = 1.2f;

    [Header("반짝임 설정")]
    [Tooltip("반짝임 효과 강도 (0이면 비활성화)")]
    [Range(0f, 1f)]
    [SerializeField] private float _sparkleStrength = 0.35f;

    [Tooltip("반짝임 속도")]
    [SerializeField] private float _sparkleSpeed = 2.5f;

    [Header("색상 설정")]
    [Tooltip("잎/빛 색상 목록 (밝고 생기 넘치는 톤)")]
    [SerializeField] private Color[] _colors = new Color[]
    {
        new Color(0.55f, 0.90f, 0.30f), // 밝은 연두
        new Color(0.40f, 0.85f, 0.45f), // 초록 빛
        new Color(0.80f, 0.95f, 0.30f), // 황록색
        new Color(1.00f, 0.88f, 0.30f), // 황금빛
        new Color(0.70f, 1.00f, 0.55f), // 연한 밝은 녹색
        new Color(0.95f, 1.00f, 0.60f), // 밝은 연노랑
        new Color(0.50f, 0.95f, 0.70f), // 민트빛
    };

    [Header("페이드 설정")]
    [Tooltip("스폰 직후 최대 알파값")]
    [Range(0f, 1f)]
    [SerializeField] private float _maxAlpha = 0.85f;

    [Tooltip("x축 도달 직전 최소 알파값")]
    [Range(0f, 1f)]
    [SerializeField] private float _minAlpha = 0.0f;

    [Header("렌더링")]
    [Tooltip("소팅 레이어 이름")]
    [SerializeField] private string _sortingLayerName = "Default";

    [Tooltip("소팅 오더")]
    [SerializeField] private int _sortingOrder = 5;

    // ──────────────────────────────────────────────
    //  내부 파티클 데이터
    // ──────────────────────────────────────────────
    private class LeafParticle
    {
        public GameObject go;
        public SpriteRenderer sr;
        public float worldX;        // 현재 X (기준)
        public float worldY;        // 현재 Y
        public float riseSpeed;
        public float wobbleOffset;
        public float sparkleOffset;
        public float sparkleSpeedMult;
        public Color baseColor;
        public float alphaScale;    // 파티클별 알파 배율
        public float spawnY;        // 스폰 시작 Y (페이드 계산용)
    }

    private readonly List<LeafParticle> _particles = new List<LeafParticle>();
    private Sprite _glowSprite;

    // ──────────────────────────────────────────────
    //  초기화
    // ──────────────────────────────────────────────
    private void Awake()
    {
        _glowSprite = CreateGlowSprite(32);

        for (int i = 0; i < _count; i++)
            SpawnParticle(randomY: true);
    }

    // ──────────────────────────────────────────────
    //  매 프레임 업데이트
    // ──────────────────────────────────────────────
    private void Update()
    {
        float t = Time.time;

        foreach (var p in _particles)
        {
            if (p.go == null) continue;

            // 상승 이동
            p.worldY += p.riseSpeed * Time.deltaTime;

            // 수평 흔들림 (잎이 바람에 살랑이는 느낌)
            float wobble = Mathf.Sin(t * _wobbleSpeed + p.wobbleOffset) * _wobbleAmplitude;
            p.go.transform.position = new Vector3(p.worldX + wobble, p.worldY, 0f);

            // x축(y=0) 도달 시 → 나무 아래에서 재스폰
            if (p.worldY >= _groundY)
            {
                ResetParticle(p, randomY: false);
                continue;
            }

            // y 진행도에 따른 페이드 (스폰 위치: 불투명 → x축: 완전 투명)
            float progress = Mathf.InverseLerp(p.spawnY, _groundY, p.worldY); // 0=스폰, 1=x축
            float baseAlpha = Mathf.Lerp(_maxAlpha, _minAlpha, progress) * p.alphaScale;

            // 반짝임 (빛의 생동감)
            float sparkle = 1f;
            if (_sparkleStrength > 0f)
            {
                float s = Mathf.Sin(t * _sparkleSpeed * p.sparkleSpeedMult + p.sparkleOffset);
                sparkle = 1f - _sparkleStrength * (1f - (s + 1f) * 0.5f);
            }

            float alpha = baseAlpha * sparkle;
            p.sr.color = new Color(p.baseColor.r, p.baseColor.g, p.baseColor.b, alpha);
        }
    }

    // ──────────────────────────────────────────────
    //  파티클 생성 및 초기화
    // ──────────────────────────────────────────────
    private void SpawnParticle(bool randomY)
    {
        var go = new GameObject("LeafParticle");
        go.transform.SetParent(transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _glowSprite;
        sr.sortingLayerName = _sortingLayerName;
        sr.sortingOrder = _sortingOrder;

        float size = Random.Range(_minSize, _maxSize);
        go.transform.localScale = Vector3.one * size;

        Color col = _colors.Length > 0 ? _colors[Random.Range(0, _colors.Length)] : Color.green;

        var p = new LeafParticle
        {
            go               = go,
            sr               = sr,
            riseSpeed        = Random.Range(_minRiseSpeed, _maxRiseSpeed),
            wobbleOffset     = Random.Range(0f, Mathf.PI * 2f),
            sparkleOffset    = Random.Range(0f, Mathf.PI * 2f),
            sparkleSpeedMult = Random.Range(0.5f, 2.0f),
            baseColor        = col,
            alphaScale       = Random.Range(0.55f, 1.0f),
        };

        ResetParticle(p, randomY);
        _particles.Add(p);
    }

    private void ResetParticle(LeafParticle p, bool randomY)
    {
        float baseX = transform.position.x;
        float baseY = transform.position.y;

        // 행복나무는 아래쪽에 위치하므로 transform.y에서 아래 방향으로 오프셋
        float minY = baseY - _spawnDepthMin;
        float maxY = baseY - _spawnDepthMax;
        // minY > maxY일 수 있으므로 정렬
        float lo = Mathf.Min(minY, maxY);
        float hi = Mathf.Max(minY, maxY);

        p.worldX       = baseX + Random.Range(-_spawnHalfWidth, _spawnHalfWidth);
        p.worldY       = randomY ? Random.Range(lo, hi) : lo;
        p.spawnY       = p.worldY;
        p.riseSpeed    = Random.Range(_minRiseSpeed, _maxRiseSpeed);
        p.wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        p.go.transform.position = new Vector3(p.worldX, p.worldY, 0f);

        // 초기 알파 적용
        float alpha = _maxAlpha * p.alphaScale;
        p.sr.color = new Color(p.baseColor.r, p.baseColor.g, p.baseColor.b, alpha);
    }

    // ──────────────────────────────────────────────
    //  부드러운 빛무리(Gaussian Glow) 스프라이트 동적 생성
    // ──────────────────────────────────────────────
    private static Sprite CreateGlowSprite(int resolution)
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
                // 빛: 중심이 밝고 가장자리로 갈수록 부드럽게 감쇠 (가우시안)
                float alpha = Mathf.Exp(-3.2f * t * t);
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
        Vector3 c = transform.position;
        float minY = c.y - _spawnDepthMin;
        float maxY = c.y - _spawnDepthMax;
        float lo = Mathf.Min(minY, maxY);
        float hi = Mathf.Max(minY, maxY);

        // 스폰 영역 (초록 박스)
        Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.4f);
        Gizmos.DrawWireCube(
            new Vector3(c.x, (lo + hi) * 0.5f, 0f),
            new Vector3(_spawnHalfWidth * 2f, hi - lo, 0f)
        );

        // 상승 목표 라인 (밝은 노란빛 = x축)
        Gizmos.color = new Color(0.9f, 0.9f, 0.2f, 0.6f);
        Gizmos.DrawLine(
            new Vector3(c.x - _spawnHalfWidth, _groundY, 0f),
            new Vector3(c.x + _spawnHalfWidth, _groundY, 0f)
        );
        Gizmos.DrawSphere(new Vector3(c.x - _spawnHalfWidth, _groundY, 0f), 0.08f);
        Gizmos.DrawSphere(new Vector3(c.x + _spawnHalfWidth, _groundY, 0f), 0.08f);
    }
}
