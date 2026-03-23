using UnityEngine;

// IPullable 상속
public class HappyFruit : MonoBehaviour, IPullable
{
    // 첫 위치 저장 변수
    private Vector2 _startPosition;
    private Rigidbody2D _fruitRigidbody;

    private void Awake()
    {
        _fruitRigidbody = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
    }

    // 무언가와 트리거 했을 때 실행되는 함수
    // target : 부딪힌 상대방을 target이라는 변수명 사용
    private void OnTriggerEnter2D(Collider2D target)
    {
        if(target.CompareTag("Boundary"))
        {
            // 부딪힌 대상이 Boundary라면 리셋
            BackToStartPoint();
        }

        else if (target.CompareTag("Seed"))
        {
            // 부딪힌 대상이 Seed라면 열매를 화면에서 사라지게 함
            gameObject.SetActive(false);
        }
    }

    // 플레이어가 잡았을 때
    public void OnPull(Transform playerHand)
    {
        _fruitRigidbody.simulated = false;
        transform.SetParent(playerHand);
        transform.localPosition = Vector2.zero;
    }

    // 플레이어가 놓았을 때
    public void OnStopP()
    {
        _fruitRigidbody.simulated = true;
        transform.SetParent(null);
    }

    // 열매 리셋하는 함수
    public void BackToStartPoint()
    {
        transform.position = _startPosition;
        _fruitRigidbody.linearVelocity = Vector2.zero;
    }
}
