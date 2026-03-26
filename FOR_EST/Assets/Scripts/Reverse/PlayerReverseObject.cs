using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class PlayerReverseObject : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    
    [Header("반전 불가능 영역 레이어 마스크")]
    [SerializeField] private LayerMask _reverseLayerMask;
    
    [Header("바닥 체크 레이어 마스크")]
    [SerializeField] private LayerMask _groundLayerMask;
    
    [field:SerializeField] public bool CanReverse { get; private set; }
    [field:SerializeField] public bool OnGround { get; private set; }
    
    private PlayerController _playerController;
    
    private void Awake()
    {
        CanReverse = true;
        OnGround = true;
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        transform.position = _player.transform.position * new Vector2(1f, -1f);
        transform.localScale = _player.transform.localScale * new Vector2(1f, -1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_reverseLayerMask & (1 << other.gameObject.layer)) != 0)
        {
            CanReverse = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if ((_reverseLayerMask & (1 << other.gameObject.layer)) != 0)
        {
            CanReverse = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((_reverseLayerMask & (1 << other.gameObject.layer)) == 0) return;
        
        CanReverse = true;
    }

    public void OnReverseGround()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
                        transform.position, 
                        0.5f, 
                        (_playerController._isReverse ? Vector2.down : Vector2.up),
                        0.5f,
                        _groundLayerMask);

        if (!hit)
        {
            OnGround = false;
        }
        else
        {
            OnGround = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }
}
