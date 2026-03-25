using UnityEngine;

public class JumpStandbyState : IState
{
    private PlayerStatus _status;
    private PlayerMovement _movement;

    public JumpStandbyState(PlayerStatus status, PlayerMovement movement)
    {
        _status = status;
        _movement = movement;
    }
    
    public void Enter()
    {
        _movement.JumpState.Value = EJumpState.Standby;
        _status.IsJumping = false;
        _status.IsFalling = false;
    }

    public void Update()
    {
        if(!_movement.IsGround())
            _movement.ChangeJumpState(_movement.Falling);
    }

    public void Exit()
    {
    }
}