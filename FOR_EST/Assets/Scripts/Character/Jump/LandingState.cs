using UnityEngine;

/// <summary>
/// 점프상태 - 착지
/// 착지 모션이 있을 경우, Enter부분에서 애니메이션 출력 후, 종료 시 standby로 전환할 것.
/// </summary>

public class LandingState : IState
{
    private CharacterStatus _status;
    private CharacterMovement _movement;

    public LandingState(CharacterStatus status, CharacterMovement movement)
    {
        _status = status;
        _movement = movement;
    }
    
    public void Enter()
    {
        _movement.JumpState.Value = EJumpState.Landing;
        _movement.ChangeJumpState(_movement.JumpStandby);
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}