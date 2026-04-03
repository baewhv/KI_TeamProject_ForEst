using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 슬픔 열매(Fruit_Sad)가 X축(바닥)에 닿아 사라질 때 재생되는 이펙트.
/// - 열매 스프라이트 복사본이 페이드 아웃
/// - 위로 퍼져나가는 가루 파티클
///
/// [사용법]
/// 1. Fruit_Sad_0 오브젝트에 이 컴포넌트를 추가합니다.
/// 2. SadFruit.cs 의 Boundary 트리거 처리 직전에 PlayEffect() 를 호출합니다.
/// 3. Inspector에서 파티클 색상, 수, 속도, 지속시간 등을 조절합니다.
/// </summary>
public class SadFruitDissolveFX : MonoBehaviour
{
    // ────────────────────────────────────────────────
    //  페이드 아웃 설정
    // ────────────────────────────────────────────────
    [Header("페이드 아웃 설정")]
    [Tooltip("열매 스프라이트 복사본이 완전히 사라지는 데 걸리는 시간 (초)")]
    [SerializeField] private float _fadeDuration = 0.5f;

    // ────────────────────────────────────────────────
    //  파티클 설정
    // ────────────────────────────────────────────────
    [Header("파티클 설정")]
    [Tooltip("스폰할 파티클 수")]
    [Range(8, 80)]
    [SerializeField] private int _particleCount = 30;

    [Tooltip("파티클 색상 목록 (먼지 느낌의 회색~어두운 계열)")]
    [SerializeField] private Color[] _colors = new Color[]
    {
        new Color(0.45f, 0.43f, 0.41f), // 따뜻한 중간 회색
        new Color(0.30f, 0.29f, 0.28f), // 짙은 회갈색
        new Color(0.55f, 0.54f, 0.52f), // 밝은 먼지색
        new Color(0.20f, 0.19f, 0.19f), // 거의 검정에 가까운 어두운 회색
        new Color(0.38f, 0.36f, 0.35f), // 중간 먼지 갈회색
        new Color(0.60f, 0.58f, 0.56f), // 연한 재색
        new Color(0.25f, 0.24f, 0.23f), // 짙은 탄회색
    };

    [Tooltip("파티클 최소 크기 (월드 유닛)")]
    [SerializeField] private float _minSize = 0.05f;

    [Tooltip("파티클 최대 크기 (월드 유닛)")]
    [SerializeField] private float _maxSize = 0.22f;

    [Tooltip("파티클 생존 시간 (초)")]
    [SerializeField] private float _particleLifetime = 0.9f;

    [Header("이동 설정")]
    [Tooltip("위로 퍼지는 최소 속도 (유닛/초)")]
    [SerializeField] private float _minUpSpeed = 1.2f;

    [Tooltip("위로 퍼지는 최대 속도 (유닛/초)")]
    [SerializeField] private float _maxUpSpeed = 3.5f;

    [Tooltip("좌우로 퍼지는 최대 수평 속도 (유닛/초)")]
    [SerializeField] private float _maxHorizontalSpeed = 1.8f;

    [Tooltip("중력 영향 (양수 = 아래로, 음수 = 위로 가속)")]
    [SerializeField] private float _gravity = -1.5f;

    [Header("스폰 오프셋")]
    [Tooltip("열매 중심에서 파티클이 스폰될 랜덤 반경")]
    [SerializeField] private float _spawnRadius = 0.3f;

    // ────────────────────────────────────────────────
    //  렌더링
    // ────────────────────────────────────────────────
    [Header("렌더링")]
    [SerializeField] private string _sortingLayerName = "Default";
    [SerializeField] private int _sortingOrder = 20;

    // ────────────────────────────────────────────────
    //  내부 상태
    // ────────────────────────────────────────────────
    private SpriteRenderer _sourceSR;
    private static Sprite _dustSprite;
    private static Material _additiveMat;

    private void Awake()
    {
        _sourceSR = GetComponent<SpriteRenderer>();

        if (_dustSprite == null)
            _dustSprite = CreateDustSprite(32);

        if (_additiveMat == null)
            _additiveMat = CreateAdditiveMaterial();
    }

    // ────────────────────────────────────────────────
    //  외부에서 호출 – SadFruit.cs 에서 Boundary 충돌 시 호출
    // ────────────────────────────────────────────────
    /// <summary>
    /// 이펙트를 재생합니다. SadFruit 이 SetActive(false) 되기 직전에 호출하세요.
    /// </summary>
    public void PlayEffect()
    {
        // 이미 비활성 상태면 무시
        if (!gameObject.activeInHierarchy) return;

        Vector3 worldPos    = transform.position;
        Vector3 fruitScale  = transform.lossyScale;   // 부모 포함 월드 스케일
        Sprite  fruitSprite = _sourceSR != null ? _sourceSR.sprite : null;
        Color   fruitColor  = _sourceSR != null ? _sourceSR.color  : Color.white;
        string  sortLayer   = _sourceSR != null ? _sourceSR.sortingLayerName : _sortingLayerName;
        int     sortOrder   = _sourceSR != null ? _sourceSR.sortingOrder     : _sortingOrder;

        // 씬 루트에 임시 이펙트 오브젝트 생성 (원본이 SetActive false 되어도 살아남음)
        var fxRoot = new GameObject("SadFruitDissolveFX_Runtime");
        fxRoot.transform.position = worldPos;

        var fx = fxRoot.AddComponent<_SadFruitFXRunner>();
        fx.Initialize(
            worldPos,
            fruitScale,
            fruitSprite,
            fruitColor,
            sortLayer,
            sortOrder,
            _fadeDuration,
            _particleCount,
            _colors,
            _minSize, _maxSize,
            _particleLifetime,
            _minUpSpeed, _maxUpSpeed,
            _maxHorizontalSpeed,
            _gravity,
            _spawnRadius,
            _sortingLayerName,
            _sortingOrder,
            _dustSprite,
            _additiveMat
        );
    }

    // ────────────────────────────────────────────────
    //  정적 에셋 생성
    // ────────────────────────────────────────────────
    private static Sprite CreateDustSprite(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float center = res * 0.5f;
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float t = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center)) / center;
                float a = Mathf.Exp(-3.5f * t * t); // 가우시안 감쇠
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    private static Material CreateAdditiveMaterial()
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        return mat;
    }
}

// ============================================================
//  내부 런너 – 씬 루트에 생성되어 이펙트를 실행 후 자기 삭제
// ============================================================
internal class _SadFruitFXRunner : MonoBehaviour
{
    // 파티클 데이터
    private struct DustParticle
    {
        public SpriteRenderer sr;
        public Vector3 pos;
        public Vector2 velocity;
        public float   lifetime;
        public float   elapsed;
        public Color   baseColor;
        public float   alphaScale;
    }

    private SpriteRenderer _ghostSR;        // 열매 스프라이트 복사본
    private float          _fadeDuration;
    private float          _fadeElapsed;

    private DustParticle[] _particles;
    private int            _aliveCount;

    private Sprite   _dustSprite;
    private Material _additiveMat;
    private string   _sortingLayer;
    private int      _sortingOrder;

    private float _gravity;

    // ────────────────────────────────
    public void Initialize(
        Vector3  worldPos,
        Vector3  fruitScale,
        Sprite   fruitSprite,
        Color    fruitColor,
        string   fruitSortLayer,
        int      fruitSortOrder,
        float    fadeDuration,
        int      particleCount,
        Color[]  colors,
        float    minSize, float maxSize,
        float    lifetime,
        float    minUpSpeed, float maxUpSpeed,
        float    maxHoriz,
        float    gravity,
        float    spawnRadius,
        string   sortLayer,
        int      sortOrder,
        Sprite   dustSprite,
        Material additiveMat)
    {
        _fadeDuration = fadeDuration;
        _gravity      = gravity;
        _dustSprite   = dustSprite;
        _additiveMat  = additiveMat;
        _sortingLayer = sortLayer;
        _sortingOrder = sortOrder;

        // 오브젝트 스케일을 파티클 크기/반경 보정에 사용 (x, y 중 큰 값 기준)
        float scaleFactor = Mathf.Max(Mathf.Abs(fruitScale.x), Mathf.Abs(fruitScale.y));
        spawnRadius *= scaleFactor;
        minSize     *= scaleFactor;
        maxSize     *= scaleFactor;

        // ── 열매 스프라이트 복사본 (페이드 아웃용) ──
        if (fruitSprite != null)
        {
            var ghostGO = new GameObject("Ghost");
            ghostGO.transform.SetParent(transform, false);
            ghostGO.transform.position   = worldPos;
            ghostGO.transform.localScale = fruitScale;  // 원본 오브젝트 스케일 그대로 적용

            _ghostSR = ghostGO.AddComponent<SpriteRenderer>();
            _ghostSR.sprite           = fruitSprite;
            _ghostSR.color            = fruitColor;
            _ghostSR.sortingLayerName = fruitSortLayer;
            _ghostSR.sortingOrder     = fruitSortOrder;
        }

        // ── 파티클 초기화 ──
        _particles  = new DustParticle[particleCount];
        _aliveCount = particleCount;

        for (int i = 0; i < particleCount; i++)
        {
            // 스폰 위치: 열매 중심 주변 랜덤
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = worldPos + new Vector3(offset.x, offset.y, 0f);

            // 속도: 위로 + 좌우 랜덤
            float upSpeed = Random.Range(minUpSpeed, maxUpSpeed);
            float hSpeed  = Random.Range(-maxHoriz, maxHoriz);
            Vector2 vel   = new Vector2(hSpeed, upSpeed);

            // 파티클 GameObject
            var go = new GameObject("Dust");
            go.transform.SetParent(transform, false);
            go.transform.position   = spawnPos;
            go.transform.localScale = Vector3.one * Random.Range(minSize, maxSize);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite           = _dustSprite;
            sr.material         = _additiveMat;
            sr.sortingLayerName = sortLayer;
            sr.sortingOrder     = sortOrder + i % 3;

            Color col = (colors != null && colors.Length > 0)
                ? colors[Random.Range(0, colors.Length)]
                : Color.white;

            float alphaScale = Random.Range(0.5f, 1.0f);
            sr.color = new Color(col.r, col.g, col.b, alphaScale);

            _particles[i] = new DustParticle
            {
                sr         = sr,
                pos        = spawnPos,
                velocity   = vel,
                lifetime   = lifetime * Random.Range(0.7f, 1.3f),
                elapsed    = 0f,
                baseColor  = col,
                alphaScale = alphaScale,
            };
        }
    }

    // ────────────────────────────────
    private void Update()
    {
        float dt = Time.deltaTime;
        bool  allDone = true;

        // ── 열매 스프라이트 페이드 아웃 ──
        if (_ghostSR != null)
        {
            _fadeElapsed += dt;
            float t = Mathf.Clamp01(_fadeElapsed / _fadeDuration);
            Color c = _ghostSR.color;
            _ghostSR.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t));

            if (t < 1f) allDone = false;
        }

        // ── 파티클 업데이트 ──
        for (int i = 0; i < _particles.Length; i++)
        {
            ref DustParticle p = ref _particles[i];
            if (p.sr == null) continue;

            p.elapsed += dt;
            if (p.elapsed >= p.lifetime)
            {
                Destroy(p.sr.gameObject);
                p.sr = null;
                continue;
            }

            allDone = false;

            // 중력 적용
            p.velocity.y += _gravity * dt;

            // 위치 이동
            p.pos += new Vector3(p.velocity.x, p.velocity.y, 0f) * dt;
            p.sr.transform.position = p.pos;

            // 진행도에 따른 페이드 아웃
            float progress = p.elapsed / p.lifetime;

            // 시작: 빠르게 나타남 → 중간: 최대 밝기 → 끝: 서서히 사라짐
            float fadeIn  = Mathf.Clamp01(progress / 0.15f);         // 0~15% 구간에서 나타남
            float fadeOut = 1f - Mathf.Clamp01((progress - 0.4f) / 0.6f); // 40~100% 구간에서 사라짐
            float alpha   = fadeIn * fadeOut * p.alphaScale;

            p.sr.color = new Color(p.baseColor.r, p.baseColor.g, p.baseColor.b, alpha);

            // 크기도 약간 줄어들며 사라지는 느낌
            float scaleMultiplier = Mathf.Lerp(1f, 0.3f, progress * progress);
            p.sr.transform.localScale = p.sr.transform.localScale.magnitude > 0
                ? p.sr.transform.localScale.normalized * (p.sr.transform.localScale.magnitude)
                : Vector3.one;
            // scale은 초기값 유지, alpha로만 처리 (연산 단순화)
        }

        // 모든 이펙트 완료 시 자기 삭제
        if (allDone)
            Destroy(gameObject);
    }
}
