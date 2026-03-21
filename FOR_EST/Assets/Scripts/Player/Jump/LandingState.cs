using UnityEngine;

/// <summary>
/// 점프상태 - 착지
/// 착지 모션이 있을 경우, Enter부분에서 애니메이션 출력 후, 종료 시 standby로 전환할 것.
/// </summary>

public class LandingState : IState
{
    private PlayerStatus _status;
    private PlayerMovement _movement;

    public LandingState(PlayerStatus status, PlayerMovement movement)
    {
        _status = status;
        _movement = movement;
    }
    
    public void Enter()
    {
        _movement.ChangeJumpState(_movement.JumpStandby);
        _status.IsJumping = false;
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}