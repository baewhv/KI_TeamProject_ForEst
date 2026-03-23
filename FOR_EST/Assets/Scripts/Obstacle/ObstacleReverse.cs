using Unity.VisualScripting;
using UnityEngine;

public class ObstacleReverse : MonoBehaviour, IReversable, IPullable
{
    private Rigidbody2D _rb;
    private ReverseObject _reverseObject;
    private bool _isPulled = false; //당겨지고 있는 상태인지 구별하는 함수 false = 안당겨지고 있음, true = 당겨지고 있음
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    public void OnPull(Transform playerHand)
    {
        _isPulled = true; //당겨지고 있는 상태면 true로 바꿔줌
    }

    public void OnStopP()
    {
        _isPulled = false; //당겨지고 있지 않은 상태면 false로 바꿔줌
    }

    public void Reverse()
    {
        if (!_reverseObject.canReverse || !_isPulled) return; //당겨지고 있지 않으면 리버스가 안되도록 함

        transform.position *= new Vector2(1f, -1f);
        transform.localScale *= new Vector2(1f, -1f);
        _rb.gravityScale *= -1f;
    }
}