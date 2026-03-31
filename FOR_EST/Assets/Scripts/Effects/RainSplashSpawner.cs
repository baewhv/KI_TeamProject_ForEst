using UnityEngine;

/// <summary>
/// 비가 지면에 닿았을 때 물 튀기기 이펙트를 생성하는 컴포넌트.
/// Ground 오브젝트에 붙이거나, 별도 오브젝트로 배치해서 사용.
/// </summary>
public class RainSplashSpawner : MonoBehaviour
{
    [Header("스플래시 이펙트 설정")]
    [Tooltip("빗방울 튀기기 파티클 프리팹 (RainSplashEffect)")]
    public GameObject splashPrefab;

    [Tooltip("초당 스플래시 생성 횟수 (비 강도에 맞게 조정)")]
    [Range(1f, 50f)]
    public float splashRate = 15f; //바닥에서 생성 빈도, 비의 강도에 맞게 조절 (예: 10~20 정도)

    [Header("스폰 영역 설정")]
    [Tooltip("지면 라인의 Y 위치 (월드 좌표)")]
    public float groundY = 0f; //지면 라인의 Y 위치, Ground 오브젝트의 Y 위치에 맞춰 설정

    [Tooltip("스플래시가 생성될 X축 범위 (중심 기준 ±halfWidth)")]
    public float halfWidth = 10f; //X축 범위, Ground 오브젝트의 너비에 맞춰 설정 (예: Ground가 20 유닛 너비면 halfWidth는 10)

    [Header("랜덤 오프셋")]
    [Tooltip("스폰 Y 위치의 랜덤 오프셋 (±)")]
    [Range(0f, 0.5f)]
    public float yJitter = 0.05f; //이펙트 생성 y축 범위 값이 크면 지면보다 좀 더 높은 곳에서 생성되고, 작으면 지면에 가까운 곳에서 생성됨

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        float interval = 1f / splashRate;

        while (_timer >= interval)
        {
            _timer -= interval;
            SpawnSplash();
        }
    }

    private void SpawnSplash()
    {
        if (splashPrefab == null) return;

        float x = transform.position.x + Random.Range(-halfWidth, halfWidth);
        float y = groundY + Random.Range(-yJitter, yJitter);
        Vector3 spawnPos = new Vector3(x, y, transform.position.z);

        GameObject splash = Instantiate(splashPrefab, spawnPos, Quaternion.identity);

        // 파티클이 끝나면 자동 제거
        ParticleSystem ps = splash.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(splash, duration);
        }
        else
        {
            Destroy(splash, 2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.5f);
        Vector3 center = new Vector3(transform.position.x, groundY, transform.position.z);
        Gizmos.DrawLine(center + Vector3.left * halfWidth, center + Vector3.right * halfWidth);
        Gizmos.DrawSphere(center + Vector3.left * halfWidth, 0.1f);
        Gizmos.DrawSphere(center + Vector3.right * halfWidth, 0.1f);
    }
}
