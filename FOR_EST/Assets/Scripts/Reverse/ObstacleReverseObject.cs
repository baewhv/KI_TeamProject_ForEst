using UnityEngine;

public class ObstacleReverseObject : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    
    [Header("반전 불가능 영역 레이어 마스크")]
    [SerializeField] private LayerMask _reverseLayerMask;
    
    [Header("바닥 체크 레이어 마스크")]
    [SerializeField] private LayerMask _groundLayerMask;
    
    [field:SerializeField] public bool canReverse { get; private set; }
    
    private void Awake()
    {
        canReverse = true;
    }

    private void Update()
    {
        transform.position = _target.transform.position * new Vector2(1f, -1f);
        transform.localScale = _target.transform.localScale * new Vector2(1f, -1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_reverseLayerMask & (1 << other.gameObject.layer)) != 0)
        {
            canReverse = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if ((_reverseLayerMask & (1 << other.gameObject.layer)) != 0)
        {
            canReverse = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((_reverseLayerMask & (1 << other.gameObject.layer)) == 0) return;
        
        canReverse = true;
    }
}
