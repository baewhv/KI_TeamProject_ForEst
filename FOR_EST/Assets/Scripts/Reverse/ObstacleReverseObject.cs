using System;
using Obstacle;
using UnityEngine;

public class ObstacleReverseObject : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    
    [Header("반전 불가능 영역 레이어 마스크")]
    [SerializeField] private LayerMask _reverseLayerMask;
    
    [Header("바닥 체크 레이어 마스크")]
    [SerializeField] private LayerMask _groundLayerMask;
    
    [field:SerializeField] public bool canReverse { get; private set; }
    [field:SerializeField] public bool OnGround { get; private set; }

    private Obstacle.Obstacle _obstacle;
    private Collider2D _collider;
    
    private void Awake()
    {
        canReverse = true;
        OnGround = true;
        _obstacle = _target.GetComponent<Obstacle.Obstacle>();
        _collider = GetComponent<Collider2D>();
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
    
    public void OnReverseGround()
    {
        float gravity = _obstacle.GetComponent<Rigidbody2D>().gravityScale;
        float checkY = (Mathf.Sign(gravity) * -1 > 0) ? _collider.bounds.min.y : _collider.bounds.max.y;
        
        Vector2 origin = new Vector2(_collider.bounds.center.x, checkY);
        Vector2 direction = (Mathf.Sign(gravity) * -1 > 0) ? Vector2.down : Vector2.up;
        Vector2 checkBoxSize = new Vector2(transform.localScale.x - 2f, 0.5f);

        RaycastHit2D hit = Physics2D.BoxCast
        (
            origin,
            checkBoxSize,
            0f,
            direction,
            0.1f,
            _groundLayerMask
        );

        if (!hit)
        {
            OnGround = false;
        }
        else
        {
            OnGround = true;
        }
    }

    public void OnDrawGizmos()
    {
        if (_obstacle == null) return;

        float gravity = _obstacle.GetComponent<Rigidbody2D>().gravityScale;
        float checkY = (Mathf.Sign(gravity) * -1 > 0) ? _collider.bounds.min.y : _collider.bounds.max.y;
        
        Vector2 origin = new Vector2(_collider.bounds.center.x, checkY);
        Vector2 direction = (Mathf.Sign(gravity) * -1 > 0) ? Vector2.down : Vector2.up;
        Vector2 checkBoxSize = new Vector2(transform.localScale.x - 2f, 0.5f);
        Vector2 targetPosition = origin + direction * 0.1f;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(origin, targetPosition);

        Gizmos.color = new Color(0, 1, 1, 0.3f); 
        Gizmos.DrawCube(targetPosition, checkBoxSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(targetPosition, checkBoxSize);
    }
}
