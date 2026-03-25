using UnityEngine;

public class FallingState : IState
{
    private PlayerStatus _status;
    private PlayerMovement _movement;

    public FallingState(PlayerStatus status, PlayerMovement movement)
    {
        _status = status;
        _movement = movement;
    }

    public void Enter()
    {
        _movement.JumpState.Value = EJumpState.Falling;
        _status.IsFalling = true;
    }

    public void Update()
    {
        if (_movement.IsGround())
        {
            _movement.ChangeJumpState(_movement.Landing);
        }
    }

    public void Exit()
    {
    }
}