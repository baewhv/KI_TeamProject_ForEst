using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStatus _status;
    private PlayerController _controller;

    public Rigidbody2D _rigidbody { get; private set; }
    private BoxCollider2D _collider; 
    
    private ContactFilter2D GroundFilter;

    private StateMachine _jumpState;
    public JumpStandbyState JumpStandby { get; private set; }
    public JumpingState Jumping { get; private set; }
    public FallingState Falling { get; private set; }
    public LandingState Landing { get; private set; }

    public void Init(PlayerStatus status)
    {
        _status = status;
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _controller = GetComponent<PlayerController>();
        _jumpState = new StateMachine();

        JumpStandby = new JumpStandbyState(_status, this);
        Jumping = new JumpingState(_status, this);
        Falling = new FallingState(_status, this);
        Landing = new LandingState(_status, this);

        _jumpState.ChangeState(JumpStandby);
    }

    void Update()
    {
        _jumpState.Update();
    }

    private void FixedUpdate()
    {
        //좌우 입력
        _rigidbody.linearVelocityX = _status.InputAxis.Value.x * _status.MoveSpeed * Time.fixedDeltaTime;
    }

    public void ChangeJumpState(IState state)
    {
        _jumpState.ChangeState(state);
    }

    public bool IsGround()
    {
        //if (_rigidbody.linearVelocityY < 0.01f) return false;
        Vector2 ray = _rigidbody.position - new Vector2(0, _collider.size.y * 0.5f);
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        if (Physics2D.Raycast(ray, _controller._isReverse ? Vector2.up : Vector2.down, GroundFilter, hits, 0.1f) > 0)
            return true;
        return false;
    }

}