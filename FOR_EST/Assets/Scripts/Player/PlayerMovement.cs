using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStatus _status;

    private Rigidbody2D _rigidbody;

    private StateMachine ReverseState;

    public void Init(PlayerStatus status)
    {
        _status = status;
        _rigidbody = GetComponent<Rigidbody2D>();
        ReverseState = new StateMachine();
    }

    void Update()
    {
        ReverseState.Update();
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocityX = _status.InputAxis.x * _status.MoveSpeed * Time.fixedDeltaTime;
        _rigidbody.AddForceY(Physics2D.gravity.y * Time.fixedDeltaTime);
    }
}