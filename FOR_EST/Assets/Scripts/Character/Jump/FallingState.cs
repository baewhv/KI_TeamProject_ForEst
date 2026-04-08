using UnityEngine;

public class FallingState : IState
{
    private CharacterStatus _status;
    private CharacterMovement _movement;

    public FallingState(CharacterStatus status, CharacterMovement movement)
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

        if (_movement.LandingReady())
        {
            _movement.Anim.SetBool("Jump", false);
        }
    }

    public void Exit()
    {
    }
}