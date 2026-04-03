using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 행복나무 잎사귀 부분에 흰색 은은한 블룸(빛나는) 이펙트를 적용합니다.
/// 3_tree_0 오브젝트(또는 그 하위)에 컴포넌트를 추가하고,
/// Inspector에서 위치 오프셋으로 행복나무 잎사귀 영역을 맞추세요.
///
/// [사용법]
/// 1. 3_tree_0 하위에 빈 GameObject를 만들어 이 컴포넌트를 붙입니다.
/// 2. '잎사귀 위치 오프셋'을 조정해 행복나무 잎 부분에 중심을 맞춥니다.
/// 3. Inspector의 각 항목으로 빛의 강도·범위·맥동·색상을 조절합니다.
/// </summary>
[DisallowMultipleComponent]
public class HappyTreeLeafBloom : MonoBehaviour
{
    // ────────────────────────────────────────────────
    //  위치 설정
    // ────────────────────────────────────────────────
    [Header("위치 설정")]
    [Tooltip("transform.position 기준 잎사귀 중심까지의 오프셋 (X, Y).\n" +
             "행복나무는 x축 아래쪽에 있으므로 Y를 음수로 조정하세요.")]
    [SerializeField] private Vector2 _leafCenterOffset = new Vector2(0f, -2.5f);

    // ────────────────────────────────────────────────
    //  블룸 핵심 파라미터
    // ────────────────────────────────────────────────
    [Header("블룸 설정")]
    [Tooltip("블룸 색상 (기본: 흰색)")]
    [SerializeField] private Color _bloomColor = new Color(1f, 1f, 1f, 1f);

    [Tooltip("블룸의 최대 알파(강도). 0이면 완전 투명, 1이면 가장 강함.")]
    [Range(0f, 1f)]
    [SerializeField] private float _intensity = 0.45f;

    [Tooltip("블룸 전체 반경 (월드 유닛). 잎사귀 크기에 맞게 조절하세요.")]
    [SerializeField] private float _radius = 3.5f;

    // ────────────────────────────────────────────────
    //  레이어드 글로우 (부드러운 블룸 느낌)
    // ────────────────────────────────────────────────
    [Header("레이어드 글로우")]
    [Tooltip("글로우 레이어 수. 많을수록 블룸이 부드럽고 자연스럽지만 Draw Call이 증가합니다.")]
    [Range(1, 5)]
    [SerializeField] private int _layerCount = 3;

    [Tooltip("외곽 레이어로 갈수록 크기가 커지는 비율 (1 = 동일, 2 = 2배 커짐).")]
    [Range(1f, 3f)]
    [SerializeField] private float _layerScaleMultiplier = 1.6f;

    [Tooltip("외곽 레이어로 갈수록 알파가 감소하는 비율 (0: 급감, 1: 동일).")]
    [Range(0f, 1f)]
    [SerializeField] private float _layerAlphaFalloff = 0.45f;

    // ────────────────────────────────────────────────
    //  맥동(Pulse) 설정
    // ────────────────────────────────────────────────
    [Header("맥동 설정")]
    [Tooltip("맥동 사용 여부. 체크 해제 시 정적 블룸으로 동작합니다.")]
    [SerializeField] private bool _enablePulse = true;

    [Tooltip("맥동 속도 (Hz 단위, 값이 클수록 빠르게 깜빡임).")]
    [Range(0.1f, 5f)]
    [SerializeField] private float _pulseSpeed = 0.8f;

    [Tooltip("맥동 강도. 0이면 변화 없음, 1이면 완전히 사라졌다 나타남.")]
    [Range(0f, 1f)]
    [SerializeField] private float _pulseStrength = 0.25f;

    [Tooltip("크기도 함께 맥동시킬지 여부.")]
    [SerializeField] private bool _pulseScale = true;

    [Tooltip("크기 맥동 강도 (반경 기준 비율, 0.1 = ±10%).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float _pulseScaleStrength = 0.08f;

    // ────────────────────────────────────────────────
    //  렌더링 설정
    // ────────────────────────────────────────────────
    [Header("렌더링")]
    [Tooltip("소팅 레이어 이름 (Sprite가 속한 레이어와 맞추거나 앞에 오도록 설정).")]
    [SerializeField] private string _sortingLayerName = "Default";

    [Tooltip("소팅 오더 (나무 스프라이트보다 앞에 보이려면 더 큰 값을 사용).")]
    [SerializeField] private int _sortingOrder = 10;

    [Tooltip("글로우 텍스처 해상도 (32~128). 클수록 부드럽지만 메모리가 늘어남).")]
    [Range(32, 512)]
    [SerializeField] private int _textureResolution = 512;

    // ────────────────────────────────────────────────
    //  내부 상태
    // ────────────────────────────────────────────────
    private readonly List<SpriteRenderer> _layers = new List<SpriteRenderer>();
    private Sprite _glowSprite;
    private Material _glowMaterial;

    // ────────────────────────────────────────────────
    //  초기화
    // ────────────────────────────────────────────────
    private void Awake()
    {
        _glowSprite   = CreateGlowSprite(_textureResolution);
        _glowMaterial = CreateAdditiveMaterial();
        BuildLayers();
    }

    private void BuildLayers()
    {
        // 기존 레이어 오브젝트 제거 (에디터에서 레이어 수 변경 대응)
        foreach (var sr in _layers)
        {
            if (sr != null) Destroy(sr.gameObject);
        }
        _layers.Clear();

        for (int i = 0; i < _layerCount; i++)
        {
            var go = new GameObject($"BloomLayer_{i}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(_leafCenterOffset.x, _leafCenterOffset.y, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite        = _glowSprite;
            sr.material      = _glowMaterial;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder  = _sortingOrder + i;

            _layers.Add(sr);
        }
    }

    // ────────────────────────────────────────────────
    //  매 프레임 업데이트
    // ────────────────────────────────────────────────
    private void Update()
    {
        float pulseFactor = 1f;
        float scaleBonus  = 0f;

        if (_enablePulse)
        {
            // sin 곡선: 0~1 범위로 정규화
            float sin = (Mathf.Sin(Time.time * _pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            pulseFactor = 1f - _pulseStrength * (1f - sin);

            if (_pulseScale)
                scaleBonus = _pulseScaleStrength * sin;
        }

        for (int i = 0; i < _layers.Count; i++)
        {
            var sr = _layers[i];
            if (sr == null) continue;

            // 레이어별 크기: 안쪽에서 바깥쪽으로 점차 커짐
            float layerScale = _radius * Mathf.Pow(_layerScaleMultiplier, i) * (1f + scaleBonus);
            sr.transform.localScale = Vector3.one * layerScale;

            // 레이어별 알파: 안쪽이 가장 밝고 바깥쪽으로 감쇠
            float layerAlpha = _intensity * Mathf.Pow(_layerAlphaFalloff, i) * pulseFactor;
            layerAlpha = Mathf.Clamp01(layerAlpha);

            sr.color = new Color(_bloomColor.r, _bloomColor.g, _bloomColor.b, layerAlpha);
            sr.transform.localPosition = new Vector3(_leafCenterOffset.x, _leafCenterOffset.y, 0f);
        }
    }

    // ────────────────────────────────────────────────
    //  에셋 생성
    // ────────────────────────────────────────────────

    /// <summary>중심이 밝고 가장자리로 부드럽게 감쇠하는 가우시안 글로우 스프라이트 생성.</summary>
    private static Sprite CreateGlowSprite(int resolution)
    {
        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float center = resolution * 0.5f;
        float r      = center;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist  = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                float t     = dist / r;
                // 가우시안 감쇠: 중심=1, 가장자리≒0
                float alpha = Mathf.Exp(-2.8f * t * t);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            resolution
        );
    }

    /// <summary>Additive 블렌딩 머티리얼 생성 (URP Unlit Transparent Additive).</summary>
    private static Material CreateAdditiveMaterial()
    {
        // URP 환경에서 사용 가능한 Sprites/Default 기반 Additive 머티리얼
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000; // Transparent queue
        return mat;
    }

    // ────────────────────────────────────────────────
    //  에디터 기즈모
    // ────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + new Vector3(_leafCenterOffset.x, _leafCenterOffset.y, 0f);

        // 최내층 블룸 범위 (진한 흰색)
        Gizmos.color = new Color(1f, 1f, 0.8f, 0.8f);
        DrawGizmosCircle(center, _radius);

        // 최외층 블룸 범위 (연한 흰색)
        if (_layerCount > 1)
        {
            float outerRadius = _radius * Mathf.Pow(_layerScaleMultiplier, _layerCount - 1);
            Gizmos.color = new Color(1f, 1f, 0.8f, 0.3f);
            DrawGizmosCircle(center, outerRadius);
        }

        // 중심점 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.12f);
    }

    private static void DrawGizmosCircle(Vector3 center, float radius, int segments = 32)
    {
        float step = Mathf.PI * 2f / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    // ────────────────────────────────────────────────
    //  에디터에서 실시간 미리보기 지원
    // ────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 레이어 수가 바뀌면 재구성 (플레이 모드 중일 때만)
        if (Application.isPlaying && _layers.Count != _layerCount)
            BuildLayers();
    }
#endif
}
