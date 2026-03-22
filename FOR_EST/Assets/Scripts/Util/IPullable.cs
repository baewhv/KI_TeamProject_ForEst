using UnityEngine;

public interface IPullable
{
    /// <summary>
    /// 당기기 상호작용
    /// </summary>
    public void OnPull(Transform playerHand);
    /// <summary>
    /// 상호작용 멈춤
    /// </summary>
    public void OnStopP();
}
