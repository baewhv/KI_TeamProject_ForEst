using UnityEngine;

/// <summary>
/// 슬픔 세상(X축 위)의 비 이펙트.
/// 일정한 속도로 위에서 아래로 낙하, 지정 Y값 도달 시 소멸.
///
/// [사용법]
/// 1. 빈 오브젝트에 이 컴포넌트를 추가합니다.
/// 2. Inspector에서 스폰 범위, 속도, 외형을 조절합니다.
/// 3. Scene 뷰에서 Gizmos로 스폰/소멸 라인을 확인할 수 있습니다.
/// </summary>
public class SadRain : MonoBehaviour
{
    // ────────────────────────────────────────────────
    //  스폰 설정
    // ────────────────────────────────────────────────
    [Header("스폰 설정")]
    [Tooltip("빗방울이 생성되는 Y 위치")]
    [SerializeField] private float _spawnY = 6f;

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
    [Tooltip("일정 낙하 속도 (유닛/초)")]
    [SerializeField] private float _fallSpeed = 10f;

    [Tooltip("이 Y값 이하로 내려가면 소멸 (바닥)")]
    [SerializeField] private float _despawnY = -1f;

    // ────────────────────────────────────────────────
    //  외형 설정
    // ────────────────────────────────────────────────
    [Header("외형 설정")]
    [SerializeField] private Color _color = new Color(0.55f, 0.75f, 1f, 0.6f);

    [Tooltip("빗방울 너비 (월드 유닛)")]
    [SerializeField] private float _dropWidth = 0.04f;

    [Tooltip("빗방울 높이 (월드 유닛)")]
    [SerializeField] private float _dropHeight = 0.35f;

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

            _pool[i].y -= _fallSpeed * dt;
            _pool[i].tr.position = new Vector3(_pool[i].tr.position.x, _pool[i].y, 0f);

            if (_pool[i].y < _despawnY)
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
            var   go = new GameObject("SadDrop");
            go.transform.SetParent(transform, false);
            go.transform.position   = new Vector3(x, _spawnY, 0f);
            go.transform.localScale = new Vector3(_dropWidth, _dropHeight, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite           = _dropSprite;
            sr.material         = _mat;
            sr.color            = _color;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder     = _sortingOrder;

            _pool[i] = new Drop { tr = go.transform, y = _spawnY };
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
        // 스폰 라인 (파란색)
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.8f);
        Gizmos.DrawLine(new Vector3(_spawnXMin, _spawnY,   0f), new Vector3(_spawnXMax, _spawnY,   0f));

        // 소멸 라인 (빨간색)
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
        Gizmos.DrawLine(new Vector3(_spawnXMin, _despawnY, 0f), new Vector3(_spawnXMax, _despawnY, 0f));
    }
}
