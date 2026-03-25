using UnityEngine;

public class ReverseObject : MonoBehaviour
{
    [SerializeField] private Transform _player;
    
    private PlayerController _playerController;
    
    [SerializeField] private LayerMask _layerMask;
    
    [field:SerializeField] public bool canReverse { get; private set; }
    
    private void Awake()
    {
        canReverse = true;
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player").transform;
        _playerController = _player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        transform.position = _player.position * new Vector2(1f, -1f);
        transform.localScale = _player.localScale * new Vector2(1f, -1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_layerMask & (1 << other.gameObject.layer)) != 0)
        {
            canReverse = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((_layerMask & (1 << other.gameObject.layer)) == 0) return;
        
        canReverse = true;
    }
}
