using System;
using System.Collections;
using UnityEngine;

public class PlayerCutSceneController : MonoBehaviour
{
    [SerializeField] private PlayerStatus _status = new PlayerStatus();

    [SerializeField] private Transform _grabPoint;
    private PlayerMovement _movement;
    private PlayerReverse _reverse;
    public bool DoAction;

    [SerializeField] private GameObject _reverseObjectPrefab;
    private PlayerReverseObject _reverseObjectScript;

    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _renderer;
    [Header("타겟도착 거리")] [SerializeField] private float CheckDistanceToTarget;
    [SerializeField] private Vector2 Target;

    [SerializeField] private LayerMask grabLayer;

    public void Init(PlayerStatus status)
    {
        _status = new PlayerStatus();
        _status.CopyStatus(status);
        _status.MoveSpeed = 50.0f;
        _movement = GetComponent<PlayerMovement>();
        _movement.Init(_status);
        _reverse = GetComponent<PlayerReverse>();
        _movement._rigidbody.gravityScale = _status.GravityScale;
        if (_reverseObjectPrefab != null) _reverseObjectPrefab = Instantiate(_reverseObjectPrefab);
        _reverseObjectScript = _reverseObjectPrefab.GetComponent<PlayerReverseObject>();
    }

    //연출 중 위치 강제 할당.
    public void SetPosition(Vector2 pos)
    {
        if (pos.y * transform.position.y < 0)
        {
            _anim.SetBool("Reverse", !_status.IsReverse);
            _status.IsReverse = !_status.IsReverse;
            _reverse.Reverse();
        }
        transform.position = pos;
        
    }

    public void SetDirection(bool isRight)
    {
        _status.IsRight = isRight;
        _renderer.flipX = !isRight;
    }

    public void SetAnimation(string anim) //연출 세팅 필요
    {
        _anim.CrossFade(anim, 0.0f);
    }

    //연출 중 이동
    public IEnumerator SetMoveTarget(Vector2 obj, bool isForceMove)
    {
        Target = obj;
        if (isForceMove) // 강제이동 시
        {
            SetPosition(obj);
            yield break;
        }
        if (obj.y * transform.position.y < 0)
        {
            Debug.LogError("연출 이동 위치가 반전위치에 있습니다. 이동을 취소합니다.");
            yield break; //서로 반대되는 지역에 있을 경우
        }
        
        float dist = Vector2.Distance(transform.position, Target);
        while (dist > CheckDistanceToTarget)
        {
            float dir = transform.position.x < Target.x ? 1 : -1;
            _status.InputAxis.Value = new Vector2(dir, 0);
            yield return null;
            dist = Vector2.Distance(transform.position, Target);
        }

        _status.InputAxis.Value = Vector2.zero;
    }

    public void SetReverse()
    {
        _reverseObjectScript.OnReverseGround();
        if (_status.IsJumping || _status.IsFalling || !_reverseObjectScript.CanReverse ||
            !_reverseObjectScript.OnGround) return;

        _anim.SetBool("Reverse", !_status.IsReverse);
        _status.IsReverse = !_status.IsReverse;
        _reverse.Reverse();
        if (_status.GrabbedObject != null)
        {
            IReversable rv = _status.GrabbedObject as IReversable;
            rv?.Reverse();
        }
    }

    public void SetGrab()
    {
        if (_status.IsGrab) //대화 시 잡기 상태를 해제해야 대화 가능
        {
            _status.GrabbedObject.OnStopPull();
            OffGrab();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * (_status.IsRight ? 1 : -1), 0.5f,
            grabLayer);
        if (hit.collider && hit.collider.CompareTag("Obstacle"))
        {
            _status.GrabbedObject = hit.collider.GetComponent<IPullable>();
            _status.GrabbedObject.OnPull(_grabPoint);
            _status.IsGrab = true;
        }
    }

    public void OffGrab()
    {
        _status.GrabbedObject = null;
        _status.IsGrab = false;
    }

    private float _currentTime;
    private float _fadeTime;

    public IEnumerator Fader(bool isFadeIn, float time)
    {
        _renderer.color = new Color(1, 1, 1, isFadeIn ? 0 : 1);
        Color color = _renderer.color;
        _fadeTime = time;
        _currentTime = 0.0f;
        while (_currentTime < _fadeTime)
        {
            _currentTime += Time.deltaTime;
            color.a += 1.0f / time * Time.deltaTime * (isFadeIn ? 1 : -1);
            _renderer.color = color;
            yield return null;
        }

        _renderer.color = new Color(1, 1, 1, isFadeIn ? 1 : 0);
    }
}