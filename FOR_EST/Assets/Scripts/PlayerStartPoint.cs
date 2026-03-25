using UnityEngine;

/// <summary>
/// StartPoint의 위치로 플레이어 오브젝트의 position을 위치시켜주는 기능만 함
/// </summary>
public class PlayerStartPoint : MonoBehaviour
{
    [SerializeField] private Vector2 _offset;

    private void Awake()
    {
        transform.position = _offset;
    }
    
    public void SpawnPoint(GameObject player)
    {
        player.transform.position = transform.position;
    }
}
