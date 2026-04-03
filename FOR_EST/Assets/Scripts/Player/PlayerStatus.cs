using UnityEngine;

/// <summary>
/// 플레이어 캐릭터 스테이터스([M]VP)
/// 기획 상 전투부분은 없어
/// 속도, 점프력 등 이동관련으로 설정.
/// </summary>
[System.Serializable]
public class PlayerStatus
{
    public ObserveValue<Vector2> InputAxis = new ObserveValue<Vector2>();
    public float MoveSpeed;
    public float JumpPower;
    public float GravityScale;

    public bool IsJumping { get; set; }
    public bool IsFalling { get; set; }
    public bool IsGrab { get; set; }
    public IPullable GrabbedObject { get; set; }
    public bool IsRight { get; set; }
    public bool IsReverse { get; set; }
    public ObserveValue<bool> OIsReverse { get; private set; } = new();
    public Vector2 BeforePosition { get; set; }

    public void CopyStatus(PlayerStatus other)
    {
        MoveSpeed = other.MoveSpeed;
        JumpPower = other.JumpPower;
        GravityScale = other.GravityScale;
    }
}