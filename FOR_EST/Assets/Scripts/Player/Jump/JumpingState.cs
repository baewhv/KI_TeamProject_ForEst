using UnityEngine;

public class JumpingState : IState
{
    private PlayerStatus _status;
    private PlayerMovement _movement;

    public JumpingState(PlayerStatus status, PlayerMovement movement)
    {
        _status = status;
        _movement = movement;
    }
    
    public void Enter()
    {
        _status.IsJumping = true;
        _movement._rigidbody.AddForceY(_status.JumpPower, ForceMode2D.Impulse);
    }

    public void Update()
    {
        if(_movement._rigidbody.linearVelocityY < 0)
            _movement.ChangeJumpState(_movement.Falling);
    }

    public void Exit()
    {
    }
}