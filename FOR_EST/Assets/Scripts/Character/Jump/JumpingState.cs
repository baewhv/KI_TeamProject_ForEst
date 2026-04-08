using UnityEngine;

public class JumpingState : IState
{
    private CharacterStatus _status;
    private CharacterMovement _movement;

    public JumpingState(CharacterStatus status, CharacterMovement movement)
    {
        _status = status;
        _movement = movement;
    }
    
    public void Enter()
    {
        
        _movement.JumpState.Value = EJumpState.Jumping;
        _status.IsJumping = true;
        
        //중가속도 운동
        float calcGravity = Mathf.Abs(Physics2D.gravity.y * _movement._rigidbody.gravityScale) ;
        float jumpImpulse = Mathf.Sqrt(2 * calcGravity * _status.JumpPower);
        Debug.Log((jumpImpulse));
        _movement._rigidbody.linearVelocityY = 0;
        _movement._rigidbody.AddForceY(_movement._rigidbody.gravityScale < 0 ? -jumpImpulse : jumpImpulse, ForceMode2D.Impulse);
    }

    public void Update()
    {
        if (!_status.IsReverse)
        {
            if(_movement._rigidbody.linearVelocityY < 0)
                _movement.ChangeJumpState(_movement.Falling);
        }
        else
        {
            if(_movement._rigidbody.linearVelocityY > 0)
                _movement.ChangeJumpState(_movement.Falling);
        }
    }

    public void Exit()
    {
    }
}