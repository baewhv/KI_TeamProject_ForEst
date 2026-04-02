using UnityEngine;

/// <summary>
/// 행복 세상(X축 아래)의 비 이펙트.
/// 지정 Y 범위에서 생성되어 초기엔 천천히, 점점 가속하며 낙하.
/// 최대 속도는 SadRain 의 _fallSpeed 와 동일하게 설정 권장.
///
/// [사용법]
/// 1. 빈 오브젝트에 이 컴포넌트를 추가합니다.
/// 2. _spawnYMin/_spawnYMax 로 생성 Y 범위를 설정합니다 (기본: 0 ~ -1).
/// 3. _despawnYMin/_despawnYMax 로 소멸 Y 범위를 설정합니다.
/// 4. _maxFallSpeed 를 SadRain 의 낙하 속도와 같게 맞춥니다.
/// 5. Scene 뷰 Gizmos 로 범위를 시각적으로 확인할 수 있습니다.
/// </summary>
public class HappyRain : MonoBehaviour
{
    // ────────────────────────────────────────────────
    //  스폰 설정
    // ────────────────────────────────────────────────
    [Header("스폰 설정")]
    [Tooltip("빗방울 생성 Y 최솟값 (행복 세상 기준 0 ~ -1 권장)")]
    [SerializeField] private float _spawnYMin = -1f;

    [Tooltip("빗방울 생성 Y 최댓값")]
    [SerializeField] private float _spawnYMax = 0f;

    [Tooltip("생성 X 최솟값")]
    [SerializeField] private float _spawnXMin = -12f;

    [Tooltip("생성 X 최댓값")]
    [SerializeField] private float _spawnXMax = 12f;

    [Tooltip("초당 생성 개수")]
    [SerializeField] private float _spawnRate = 40f;

    // ────────────────────────────────────────────────
    //  낙하 설정
    // ────────────────────────────────────────────────
    [Header("낙하 설정")]
    [Tooltip("생성 직후 중력 크기 (낮을수록 처음에 더 느리게 떨어짐)")]
    [SerializeField] private float _initialGravity = 1f;

    [Tooltip("가속이 완료된 후의 목표 중력값")]
    [SerializeField] private float _targetGravity = 15f;

    [Tooltip("초기 중력 → 목표 중력까지 걸리는 시간 (초)")]
    [SerializeField] private float _gravityRampTime = 1.5f;

    [Tooltip("최대 낙하 속도 — SadRain 의 _fallSpeed 와 동일하게 설정 권장 (유닛/초)")]
    [SerializeField] private float _maxFallSpeed = 10f;

    // ────────────────────────────────────────────────
    //  소멸 범위
    // ────────────────────────────────────────────────
    [Header("소멸 범위")]
    [Tooltip("이 Y값 이하로 내려가면 소멸")]
    [SerializeField] private float _despawnYMin = -20f;

    [Tooltip("이 Y값 이상으로 올라가면 소멸 (범위 이탈 방지)")]
    [SerializeField] private float _despawnYMax = 1f;

    // ────────────────────────────────────────────────
    //  외형 설정
    // ────────────────────────────────────────────────
    [Header("외형 설정")]
    [SerializeField] private Color _color = new Color(1f, 0.9f, 0.5f, 0.6f);

    [Tooltip("빗방울 너비 (월드 유닛)")]
    [SerializeField] private float _dropWidth = 0.04f;

    [Tooltip("빗방울 높이 (월드 유닛)")]
    [SerializeField] private float _dropHeight = 0.28f;

    // ────────────────────────────────────────────────
    //  성능 / 렌더링
    // ────────────────────────────────────────────────
    [Header("성능 설정")]
    [Tooltip("오브젝트 풀 크기. 최대 동시 빗방울 수")]
    [SerializeField] private int _poolSize = 300;

    [Header("렌더링")]
    [SerializeField] private string _sortingLayerName = "Default";
    [SerializeField] private int _sortingOrder = 10;

    // ────────────────────────────────────────────────
    //  내부 상태
    // ────────────────────────────────────────────────
    private struct Drop
    {
        public Transform tr;
        public float     y;
        public float     velocity;  // 현재 낙하 속도 (유닛/초)
        public float     elapsed;   // 생성 후 경과 시간
    }

    private Drop[]  _pool;
    private float   _spawnTimer;
    private static Sprite   _dropSprite;
    private static Material _mat;

    private void Awake()
    {
        _pool = new Drop[_poolSize];
        if (_dropSprite == null) _dropSprite = CreateSprite();
        if (_mat == null)        _mat = new Material(Shader.Find("Sprites/Default"));
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 스폰
        _spawnTimer += dt;
        float interval = 1f / Mathf.Max(_spawnRate, 0.001f);
        while (_spawnTimer >= interval)
        {
            _spawnTimer -= interval;
            TrySpawn();
        }

        // 이동 + 소멸
        for (int i = 0; i < _poolSize; i++)
        {
            if (_pool[i].tr == null) continue;

            _pool[i].elapsed += dt;

            // 경과 시간에 따라 중력 선형 보간 (initialGravity → targetGravity)
            float t       = Mathf.Clamp01(_pool[i].elapsed / _gravityRampTime);
            float gravity = Mathf.Lerp(_initialGravity, _targetGravity, t);

            // 속도 증가 (최대 속도 제한)
            _pool[i].velocity = Mathf.Min(_pool[i].velocity + gravity * dt, _maxFallSpeed);

            _pool[i].y -= _pool[i].velocity * dt;
            _pool[i].tr.position = new Vector3(_pool[i].tr.position.x, _pool[i].y, 0f);

            // 소멸 범위 이탈 시 제거
            if (_pool[i].y < _despawnYMin || _pool[i].y > _despawnYMax)
            {
                Destroy(_pool[i].tr.gameObject);
                _pool[i].tr = null;
            }
        }
    }

    private void TrySpawn()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            if (_pool[i].tr != null) continue;

            float x  = Random.Range(_spawnXMin, _spawnXMax);
            float y  = Random.Range(_spawnYMin, _spawnYMax);
            var   go = new GameObject("HappyDrop");
            go.transform.SetParent(transform, false);
            go.transform.position   = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(_dropWidth, _dropHeight, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite           = _dropSprite;
            sr.material         = _mat;
            sr.color            = _color;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder     = _sortingOrder;

            _pool[i] = new Drop { tr = go.transform, y = y, velocity = 0f, elapsed = 0f };
            return;
        }
    }

    private static Sprite CreateSprite()
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

    private void OnDrawGizmosSelected()
    {
        // 스폰 Y 범위 (노란색)
        Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.8f);
        Gizmos.DrawLine(new Vector3(_spawnXMin, _spawnYMin, 0f), new Vector3(_spawnXMax, _spawnYMin, 0f));
        Gizmos.DrawLine(new Vector3(_spawnXMin, _spawnYMax, 0f), new Vector3(_spawnXMax, _spawnYMax, 0f));

        // 소멸 Y 범위 (빨간색)
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
        Gizmos.DrawLine(new Vector3(_spawnXMin, _despawnYMin, 0f), new Vector3(_spawnXMax, _despawnYMin, 0f));
        Gizmos.DrawLine(new Vector3(_spawnXMin, _despawnYMax, 0f), new Vector3(_spawnXMax, _despawnYMax, 0f));
    }
}
