using UnityEngine;

public abstract class BaseInteractionObject : MonoBehaviour, IPullable, IRespawnable
{
    protected Vector2 _spawnPos;
    protected Transform _playerHand;
    protected Rigidbody2D _rb;
    protected bool _isPulling = false;

    public virtual void Init()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spawnPos = transform.position;
    }
    
    public virtual void OnPull(Transform playerHand)
    {
        _isPulling = true;
        _playerHand = playerHand;
    }

    public virtual void OnStopP()
    {
        _isPulling = false;
        _playerHand = null;
    }

    public virtual void Respawn()
    {
        transform.position = _spawnPos;
    }
}
