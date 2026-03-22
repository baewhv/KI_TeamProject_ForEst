using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReverse : MonoBehaviour, IReversable
{
    private Rigidbody2D _rb;
    private ReverseObject _reverseObject;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _reverseObject = FindAnyObjectByType<ReverseObject>();
    }
    
    public void Reverse()
    {
        if (!_reverseObject.canReverse) return;
        
        transform.position *= new Vector2(1f, -1f);
        transform.localScale *= new Vector2(1f, -1f);
        _rb.gravityScale *= -1f;
    }
}
