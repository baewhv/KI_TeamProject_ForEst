using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화면에 반딧불이 떠다니는 빛무리 이펙트를 생성하는 컴포넌트.
/// 카메라를 따라다니며 항상 화면 안에 유지됩니다.
/// 씬의 빈 오브젝트에 붙여 사용하세요.
/// </summary>
public class FireflyEffect : MonoBehaviour
{
    [Header("생성 설정")]
    [Tooltip("화면에 동시에 존재하는 반딧불 수")]
    [Range(5, 100)]
    [SerializeField] private int _count = 30;

    [Tooltip("반딧불 스프라이트 프리팹 (비워두면 자동으로 원형 스프라이트 사용)")]
    [SerializeField] private GameObject _fireflyPrefab;

    [Header("크기 설정")]
    [Tooltip("반딧불 최소 크기")]
    [SerializeField] private float _minSize = 0.04f;

    [Tooltip("반딧불 최대 크기")]
    [SerializeField] private float _maxSize = 0.12f;

    [Header("이동 설정")]
    [Tooltip("기본 이동 속도")]
    [SerializeField] private float _moveSpeed = 0.4f;

    [Tooltip("사인파 흔들림 강도")]
    [SerializeField] private float _wobbleAmplitude = 0.6f;

    [Tooltip("사인파 흔들림 속도")]
    [SerializeField] private float _wobbleSpeed = 1.2f;

    [Header("깜빡임 설정")]
    [Tooltip("최소 알파 (어두울 때)")]
    [Range(0f, 1f)]
    [SerializeField] private float _minAlpha = 0.2f;

    [Tooltip("최대 알파 (밝을 때)")]
    [Range(0f, 1f)]
    [SerializeField] private float _maxAlpha = 0.9f;

    [Tooltip("깜빡임 속도")]
    [SerializeField] private float _blinkSpeed = 1.5f;

    [Header("색상 설정")]
    [Tooltip("반딧불 색상 목록 (랜덤 선택)")]
    [SerializeField] private Color[] _colors = new Color[]
    {
        new Color(0.8f, 1f, 0.4f),   // 연두빛
        new Color(0.6f, 1f, 0.6f),   // 초록빛
        new Color(1f,  0.95f, 0.4f), // 노란빛
        new Color(0.4f, 1f, 0.9f),   // 청록빛
        new Color(0.9f, 1f, 0.7f),   // 연한 연두
    };

    [Header("스폰 범위 (카메라 기준 월드 단위)")]
    [Tooltip("카메라 뷰 크기 대비 스폰 여유 (1 = 화면 딱 맞게)")]
    [SerializeField] private float _spawnMargin = 1.05f;

    // ──────────────────────────────────────────────
    //  내부 반딧불 데이터
    // ──────────────────────────────────────────────
    private class Firefly
    {
        public GameObject go;
        public SpriteRenderer sr;
        public Vector3 velocity;
        public float wobbleOffsetX;
        public float wobbleOffsetY;
        public float blinkOffset;
        public float blinkSpeedMult;
        public Color baseColor;

        // 순간 깜빡임 (반딧불 특유의 불규칙 점등)
        public bool isFlashing;
        public float flashTimer;
        public float flashDuration;
        public float flashCooldown;
        public float flashCooldownTimer;
    }

    private readonly List<Firefly> _fireflies = new List<Firefly>();
    private Camera _cam;
    private Sprite _circleSprite;

    // ──────────────────────────────────────────────
    //  초기화
    // ──────────────────────────────────────────────
    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null)
            _cam = FindFirstObjectByType<Camera>();

        if (_fireflyPrefab == null)
            _circleSprite = CreateCircleSprite(32);

        for (int i = 0; i < _count; i++)
            SpawnFirefly(randomPosition: true);
    }

    // ──────────────────────────────────────────────
    //  매 프레임 업데이트
    // ──────────────────────────────────────────────
    private void Update()
    {
        if (_cam == null) return;

        float halfH = _cam.orthographicSize * _spawnMargin;
        float halfW = halfH * _cam.aspect;
        Vector3 camPos = _cam.transform.position;

        float t = Time.time;

        foreach (var f in _fireflies)
        {
            if (f.go == null) continue;

            // ── 이동 ──
            float wx = Mathf.Sin(t * _wobbleSpeed + f.wobbleOffsetX) * _wobbleAmplitude;
            float wy = Mathf.Cos(t * _wobbleSpeed + f.wobbleOffsetY) * _wobbleAmplitude;
            Vector3 move = (f.velocity + new Vector3(wx, wy, 0f)) * Time.deltaTime;
            f.go.transform.position += move;

            // ── 화면 밖으로 나가면 반대편에서 재진입 ──
            Vector3 pos = f.go.transform.position;
            float minX = camPos.x - halfW;
            float maxX = camPos.x + halfW;
            float minY = camPos.y - halfH;
            float maxY = camPos.y + halfH;

            if (pos.x < minX) pos.x = maxX;
            else if (pos.x > maxX) pos.x = minX;
            if (pos.y < minY) pos.y = maxY;
            else if (pos.y > maxY) pos.y = minY;
            f.go.transform.position = pos;

            // ── 깜빡임 ──
            float alpha;

            if (f.isFlashing)
            {
                // 순간 점등 후 서서히 꺼짐
                f.flashTimer += Time.deltaTime;
                float ratio = 1f - (f.flashTimer / f.flashDuration);
                alpha = Mathf.Lerp(_minAlpha * 0.1f, _maxAlpha, ratio);

                if (f.flashTimer >= f.flashDuration)
                {
                    f.isFlashing = false;
                    f.flashCooldownTimer = 0f;
                    f.flashCooldown = Random.Range(1f, 4f);
                }
            }
            else
            {
                // 기본 사인파 깜빡임
                float blink = Mathf.Sin(t * _blinkSpeed * f.blinkSpeedMult + f.blinkOffset);
                alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (blink + 1f) * 0.5f);

                // 쿨다운 이후 랜덤 순간 점등 발생
                f.flashCooldownTimer += Time.deltaTime;
                if (f.flashCooldownTimer >= f.flashCooldown && Random.value < 0.004f)
                {
                    f.isFlashing = true;
                    f.flashTimer = 0f;
                    f.flashDuration = Random.Range(0.08f, 0.25f);
                }
            }

            f.sr.color = new Color(f.baseColor.r, f.baseColor.g, f.baseColor.b, alpha);
        }
    }

    // ──────────────────────────────────────────────
    //  반딧불 생성
    // ──────────────────────────────────────────────
    private void SpawnFirefly(bool randomPosition)
    {
        GameObject go;

        if (_fireflyPrefab != null)
        {
            go = Instantiate(_fireflyPrefab, transform);
        }
        else
        {
            go = new GameObject("Firefly");
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _circleSprite;
        }

        go.transform.position = randomPosition ? GetRandomScreenPos() : GetEdgeScreenPos();

        float size = Random.Range(_minSize, _maxSize);
        go.transform.localScale = Vector3.one * size;

        var firefly = new Firefly
        {
            go  = go,
            sr  = go.GetComponent<SpriteRenderer>(),
        };

        if (firefly.sr == null)
            firefly.sr = go.GetComponentInChildren<SpriteRenderer>();

        // 랜덤 색상
        Color col = _colors.Length > 0
            ? _colors[Random.Range(0, _colors.Length)]
            : Color.white;
        firefly.baseColor = col;

        // 이동 속도 (랜덤 방향)
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float speed = Random.Range(_moveSpeed * 0.4f, _moveSpeed);
        firefly.velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f);

        // 흔들림/깜빡임 오프셋 (제각각 다르게)
        firefly.wobbleOffsetX    = Random.Range(0f, Mathf.PI * 2f);
        firefly.wobbleOffsetY    = Random.Range(0f, Mathf.PI * 2f);
        firefly.blinkOffset      = Random.Range(0f, Mathf.PI * 2f);
        firefly.blinkSpeedMult   = Random.Range(0.5f, 2f);
        firefly.flashCooldown    = Random.Range(1f, 4f);
        firefly.flashCooldownTimer = Random.Range(0f, 4f);

        // 렌더 레이어 설정 (다른 오브젝트 위에 표시)
        if (firefly.sr != null)
        {
            firefly.sr.sortingLayerName = "Default";
            firefly.sr.sortingOrder     = 10;
            firefly.sr.color = new Color(col.r, col.g, col.b, _minAlpha);
        }

        _fireflies.Add(firefly);
    }

    // ──────────────────────────────────────────────
    //  위치 헬퍼
    // ──────────────────────────────────────────────
    private Vector3 GetRandomScreenPos()
    {
        if (_cam == null) return Vector3.zero;
        float halfH = _cam.orthographicSize * _spawnMargin;
        float halfW = halfH * _cam.aspect;
        Vector3 c = _cam.transform.position;
        return new Vector3(
            c.x + Random.Range(-halfW, halfW),
            c.y + Random.Range(-halfH, halfH),
            0f);
    }

    private Vector3 GetEdgeScreenPos()
    {
        // 화면 가장자리에서 생성 (루프 시 사용)
        return GetRandomScreenPos();
    }

    // ──────────────────────────────────────────────
    //  원형 스프라이트 동적 생성 (프리팹 없을 때)
    // ──────────────────────────────────────────────
    private static Sprite CreateCircleSprite(int resolution)
    {
        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = resolution * 0.5f;
        float r      = center;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                // 부드러운 Gaussian 빛무리
                float alpha = Mathf.Exp(-3.5f * (dist / r) * (dist / r));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            resolution);
    }

    // ──────────────────────────────────────────────
    //  에디터 기즈모
    // ──────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfH = cam.orthographicSize * _spawnMargin;
        float halfW = halfH * cam.aspect;
        Vector3 c   = cam.transform.position;
        c.z = 0f;

        Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.4f);
        Gizmos.DrawWireCube(c, new Vector3(halfW * 2f, halfH * 2f, 0f));
    }
}
