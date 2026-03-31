using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 슬픔나무에서 불에 타고 남은 재, 먼지 파티클이 생성되어 x축(y=0)까지 떨어지는 이펙트.
/// 슬픔나무 오브젝트 또는 대응하는 빈 오브젝트에 붙여 사용하세요.
/// transform.position을 나무 뿌리 기준점으로 맞추고 높이 오프셋을 조정하세요.
/// </summary>
public class SadnessTreeEffect : MonoBehaviour
{
    [Header("파티클 생성 설정")]
    [Tooltip("동시에 존재하는 재/먼지 파티클 수")]
    [Range(10, 300)]
    [SerializeField] private int _count = 80;

    [Header("나무 영역 설정")]
    [Tooltip("파티클 스폰 X 범위 (transform 중심 기준 ±)")]
    [SerializeField] private float _spawnHalfWidth = 2.5f;

    [Tooltip("스폰 최소 높이 오프셋 (transform.y 기준 위로, 나무 아랫부분)")]
    [SerializeField] private float _spawnHeightMin = 0.5f;

    [Tooltip("스폰 최대 높이 오프셋 (transform.y 기준 위로, 나무 꼭대기)")]
    [SerializeField] private float _spawnHeightMax = 5.5f;

    [Header("낙하 목표")]
    [Tooltip("낙하 목표 Y (x축 위치, 기본 0)")]
    [SerializeField] private float _groundY = 0f;

    [Header("파티클 크기")]
    [Tooltip("최소 파티클 크기")]
    [SerializeField] private float _minSize = 0.03f;

    [Tooltip("최대 파티클 크기")]
    [SerializeField] private float _maxSize = 0.12f;

    [Header("낙하 속도")]
    [Tooltip("최소 낙하 속도 (유닛/초)")]
    [SerializeField] private float _minFallSpeed = 0.3f;

    [Tooltip("최대 낙하 속도 (유닛/초)")]
    [SerializeField] private float _maxFallSpeed = 1.4f;

    [Header("흔들림 설정")]
    [Tooltip("수평 흔들림 강도 (재가 바람에 떠다니는 정도)")]
    [SerializeField] private float _wobbleAmplitude = 0.18f;

    [Tooltip("흔들림 속도")]
    [SerializeField] private float _wobbleSpeed = 0.9f;

    [Header("색상 설정")]
    [Tooltip("재/먼지 색상 목록 (어두운 탄 느낌)")]
    [SerializeField] private Color[] _colors = new Color[]
    {
        new Color(0.12f, 0.10f, 0.09f), // 거의 검정 (탄 재)
        new Color(0.22f, 0.20f, 0.18f), // 짙은 회색
        new Color(0.35f, 0.30f, 0.27f), // 회색 (먼지)
        new Color(0.18f, 0.16f, 0.14f), // 어두운 갈색-회색
        new Color(0.42f, 0.37f, 0.32f), // 연한 재색
        new Color(0.08f, 0.07f, 0.06f), // 검정 숯
    };

    [Header("페이드 설정")]
    [Tooltip("스폰 직후 최대 알파값")]
    [Range(0f, 1f)]
    [SerializeField] private float _maxAlpha = 0.80f;

    [Tooltip("바닥 도달 직전 최소 알파값")]
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
    private class AshParticle
    {
        public GameObject go;
        public SpriteRenderer sr;
        public float worldX;       // 현재 X (기준)
        public float worldY;       // 현재 Y
        public float fallSpeed;
        public float wobbleOffset;
        public Color baseColor;
        public float alphaScale;   // 파티클별 알파 배율
        public float spawnY;       // 스폰 시작 Y (페이드 계산용)
    }

    private readonly List<AshParticle> _particles = new List<AshParticle>();
    private Sprite _circleSprite;

    // ──────────────────────────────────────────────
    //  초기화
    // ──────────────────────────────────────────────
    private void Awake()
    {
        _circleSprite = CreateCircleSprite(24);

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

            // 낙하 이동
            p.worldY -= p.fallSpeed * Time.deltaTime;

            // 수평 흔들림 (재가 공기 중에 흔들리는 느낌)
            float wobble = Mathf.Sin(t * _wobbleSpeed + p.wobbleOffset) * _wobbleAmplitude;
            p.go.transform.position = new Vector3(p.worldX + wobble, p.worldY, 0f);

            // 바닥(y=0) 도달 시 → 나무 위에서 재스폰
            if (p.worldY <= _groundY)
            {
                ResetParticle(p, randomY: false);
                continue;
            }

            // y 진행도에 따른 페이드 (스폰 위치: 불투명 → 바닥: 완전 투명)
            float progress = Mathf.InverseLerp(p.spawnY, _groundY, p.worldY); // 0=스폰, 1=바닥
            float alpha = Mathf.Lerp(_maxAlpha, _minAlpha, progress) * p.alphaScale;
            p.sr.color = new Color(p.baseColor.r, p.baseColor.g, p.baseColor.b, alpha);
        }
    }

    // ──────────────────────────────────────────────
    //  파티클 생성 및 초기화
    // ──────────────────────────────────────────────
    private void SpawnParticle(bool randomY)
    {
        var go = new GameObject("AshParticle");
        go.transform.SetParent(transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _circleSprite;
        sr.sortingLayerName = _sortingLayerName;
        sr.sortingOrder = _sortingOrder;

        float size = Random.Range(_minSize, _maxSize);
        go.transform.localScale = Vector3.one * size;

        Color col = _colors.Length > 0 ? _colors[Random.Range(0, _colors.Length)] : Color.gray;

        var p = new AshParticle
        {
            go          = go,
            sr          = sr,
            fallSpeed   = Random.Range(_minFallSpeed, _maxFallSpeed),
            wobbleOffset = Random.Range(0f, Mathf.PI * 2f),
            baseColor   = col,
            alphaScale  = Random.Range(0.5f, 1.0f),
        };

        ResetParticle(p, randomY);
        _particles.Add(p);
    }

    private void ResetParticle(AshParticle p, bool randomY)
    {
        float baseX = transform.position.x;
        float baseY = transform.position.y;

        float minY = baseY + _spawnHeightMin;
        float maxY = baseY + _spawnHeightMax;

        p.worldX      = baseX + Random.Range(-_spawnHalfWidth, _spawnHalfWidth);
        p.worldY      = randomY ? Random.Range(minY, maxY) : maxY;
        p.spawnY      = p.worldY;
        p.fallSpeed   = Random.Range(_minFallSpeed, _maxFallSpeed);
        p.wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        p.go.transform.position = new Vector3(p.worldX, p.worldY, 0f);

        // 초기 알파 적용
        float alpha = _maxAlpha * p.alphaScale;
        p.sr.color = new Color(p.baseColor.r, p.baseColor.g, p.baseColor.b, alpha);
    }

    // ──────────────────────────────────────────────
    //  원형(가우시안 글로우) 스프라이트 동적 생성
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
                // 재/먼지: 좀 더 날카로운 가장자리 (선명한 입자 느낌)
                float alpha = dist < r ? Mathf.Pow(Mathf.Max(0f, 1f - t), 1.8f) : 0f;
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
        float minY = c.y + _spawnHeightMin;
        float maxY = c.y + _spawnHeightMax;

        // 스폰 영역 (회색 박스)
        Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
        Gizmos.DrawWireCube(
            new Vector3(c.x, (minY + maxY) * 0.5f, 0f),
            new Vector3(_spawnHalfWidth * 2f, maxY - minY, 0f)
        );

        // 낙하 목표 라인 (빨간색 = 바닥/x축)
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawLine(
            new Vector3(c.x - _spawnHalfWidth, _groundY, 0f),
            new Vector3(c.x + _spawnHalfWidth, _groundY, 0f)
        );
        Gizmos.DrawSphere(new Vector3(c.x - _spawnHalfWidth, _groundY, 0f), 0.08f);
        Gizmos.DrawSphere(new Vector3(c.x + _spawnHalfWidth, _groundY, 0f), 0.08f);
    }
}
