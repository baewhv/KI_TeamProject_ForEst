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
    [SerializeField] private GameObject Target;

    [SerializeField] private LayerMask grabLayer;

    public void Init(PlayerStatus status)
    {
        _status = status;
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
    public void SetMoveTarget(Vector2 obj)
    {
        Target.transform.position = obj;
    }

    public void SetReverse()
    {
        _reverseObjectScript.OnReverseGround();
        if (_status.IsJumping || _status.IsFalling || !_reverseObjectScript.CanReverse ||
            !_reverseObjectScript.OnGround) return;

        _anim.SetBool("Reverse", _status.IsReverse);
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
            Debug.Log(_renderer.color);
            yield return null;
        }
        _renderer.color = new Color(1, 1, 1, isFadeIn ? 1 : 0);
    }
}