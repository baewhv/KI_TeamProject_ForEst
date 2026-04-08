using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterReverse : MonoBehaviour, IReversable
{
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    
    public void Reverse()
    {
        transform.position *= new Vector2(1f, -1f);
        transform.localScale *= new Vector2(1f, -1f);
        _rb.gravityScale *= -1f;
    }
}
