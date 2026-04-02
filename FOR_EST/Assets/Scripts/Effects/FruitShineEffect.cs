using UnityEngine;

/// <summary>
/// 열매 뒤에서 달빛처럼 은은하게 빛나며 시계 방향으로 천천히 회전하는 이펙트.
/// isSadFruit = true  → 무채색 어두운 계열 (회색~검정)
/// isSadFruit = false → 무채색 밝은 계열 (회색~흰색)
/// Inspector 에서 ellipseRatio 를 조절하면 즉시 모양이 변경됩니다.
/// </summary>
public class FruitShineEffect : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector 노출 파라미터
    // ─────────────────────────────────────────────

    [Header("효과 타입")]
    [Tooltip("true = 슬픈 열매 (어두운 무채색) / false = 행복 열매 (밝은 무채색)")]
    public bool isSadFruit = true;

    [Header("글로우 모양")]
    [Tooltip("단축/장축 비율. 1.0 = 원형, 낮을수록 납작한 타원")]
    [Range(0.3f, 1.0f)]
    public float ellipseRatio = 0.6f;

    [Tooltip("텍스처 해상도 (2의 제곱 권장)")]
    public int textureResolution = 128;

    [Header("크기")]
    [Tooltip("이펙트 오브젝트의 로컬 스케일 배수")]
    public float effectScale = 3.5f;

    [Header("회전")]
    [Tooltip("초당 회전 각도 (양수 = 시계 방향)")]
    public float rotationSpeed = 35f;

    [Header("렌더링")]
    [Tooltip("과일 SpriteRenderer(5)보다 낮게 설정해 뒤에 렌더링")]
    public int sortingOrder = 3;
    public string sortingLayerName = "Default";

    // ─────────────────────────────────────────────
    //  색상
    // ─────────────────────────────────────────────
    private static readonly Color _sadColor   = new Color(0.30f, 0.30f, 0.30f, 1f);
    private static readonly Color _happyColor = new Color(0.90f, 0.90f, 0.90f, 1f);

    // ─────────────────────────────────────────────
    //  내부 변수
    // ─────────────────────────────────────────────
    private GameObject     _glowObject;
    private SpriteRenderer _glowRenderer;
    private float          _builtRatio = -1f;   // 마지막으로 텍스처를 빌드한 ratio 값

    // ─────────────────────────────────────────────
    //  Unity 생명 주기
    // ─────────────────────────────────────────────

    void Start()
    {
        BuildEffect();
    }

    void Update()
    {
        if (_glowObject == null) return;

        // 시계 방향 회전 (Z축 음수 방향)
        _glowObject.transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

        // 런타임 중 ellipseRatio 변경 감지 → 텍스처 재생성
        if (!Mathf.Approximately(_builtRatio, ellipseRatio))
            RebuildTexture();
    }

    /// <summary>
    /// Inspector 에서 값을 변경할 때마다 호출 → 에디터 프리뷰 즉시 반영
    /// </summary>
    void OnValidate()
    {
        if (_glowRenderer == null) return;
        RebuildTexture();
    }

    // ─────────────────────────────────────────────
    //  이펙트 빌드 / 텍스처 갱신
    // ─────────────────────────────────────────────

    void BuildEffect()
    {
        _glowObject = new GameObject("MoonGlow");
        _glowObject.transform.SetParent(transform);
        _glowObject.transform.localPosition = Vector3.zero;
        _glowObject.transform.localScale    = Vector3.one * effectScale;

        _glowRenderer = _glowObject.AddComponent<SpriteRenderer>();
        _glowRenderer.sortingLayerName = sortingLayerName;
        _glowRenderer.sortingOrder     = sortingOrder;

        // 가산 혼합 재질
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        _glowRenderer.material = mat;

        RebuildTexture();
    }

    void RebuildTexture()
    {
        Color glowColor = isSadFruit ? _sadColor : _happyColor;
        Texture2D tex = CreateMoonGlowTexture(textureResolution, ellipseRatio, glowColor);
        _glowRenderer.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            tex.width
        );
        _builtRatio = ellipseRatio;
    }

    // ─────────────────────────────────────────────
    //  텍스처 생성 — 달빛 타원 글로우
    // ─────────────────────────────────────────────

    static Texture2D CreateMoonGlowTexture(int size, float ratio, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float   half   = size * 0.5f;
        Color[] pixels = new Color[size * size];

        const float sigma = 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - half) / half;
                float ny = (y - half) / half;

                // 타원 거리: Y축을 ratio 로 나눠 원으로 환산 후 거리 계산
                float ex = nx;
                float ey = ny / ratio;
                float ellipseDist = Mathf.Sqrt(ex * ex + ey * ey);

                if (ellipseDist >= 1f)
                {
                    pixels[y * size + x] = Color.clear;
                    continue;
                }

                // 가우시안 알파 — 중앙 밝고 가장자리로 부드럽게 감쇠
                float alpha = Mathf.Exp(-(ellipseDist * ellipseDist) / (2f * sigma * sigma));

                pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
