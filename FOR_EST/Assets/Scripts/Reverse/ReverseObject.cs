using UnityEngine;

public class ReverseObject : MonoBehaviour
{
    [SerializeField] private Transform _player;
    
    public bool canReverse;
    
    private void Awake()
    {
        canReverse = true;
    }

    private void Update()
    {
        transform.position = _player.position * new Vector2(1f, -1f);
        transform.localScale = _player.localScale * new Vector2(1f, -1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            canReverse = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Map")) return;
        
        canReverse = true;
    }
}
