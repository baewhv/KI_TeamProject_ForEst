using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 조작 부분
/// </summary>
public class PlayerController : MonoBehaviour, IRespawnable
{
    [SerializeField] private PlayerStatus _status = new PlayerStatus();
    public PlayerStatus GetStatus => _status;
    [SerializeField] private Transform _grabPoint;
    private PlayerMovement _movement;
    private PlayerReverse _reverse;
    private ReverseView _reverseView;

    private UserInput _input;
    private GameObject _grabObject;
    public bool IsReverse => _status.IsReverse;

    [SerializeField] private LayerMask grabLayer;

    [SerializeField] private GameObject _reverseObjectPrefab;
    private PlayerReverseObject _reverseObjectScript;

    private Animator _anim;
    private SpriteRenderer _renderer;

    private Vector2 _spawnPos;
    private bool _isRespawning;
    private bool _rightObject;

    private void Awake()
    {
        _input = new UserInput();
        _movement = GetComponent<PlayerMovement>();
        _movement.Init(_status);
        _reverse = GetComponent<PlayerReverse>();
        _reverseView = GetComponentInChildren<ReverseView>();
        _status.InputAxis.AddListener(SetDirection);
        _movement._rigidbody.gravityScale = _status.GravityScale;
        _status.IsReverse = false;
        if (_reverseObjectPrefab != null) _reverseObjectPrefab = Instantiate(_reverseObjectPrefab);
        _reverseObjectScript = _reverseObjectPrefab.GetComponent<PlayerReverseObject>();
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _spawnPos = transform.position;

        _status.IsReverse = transform.position.y < 0;
        _movement._rigidbody.gravityScale = _status.GravityScale * (_status.IsReverse ? -1 : 1);
        transform.localScale *= new Vector2(1f, _status.IsReverse ? -1 : 1);
        _anim.SetBool("Reverse", _status.IsReverse);
    }

    private void OnEnable()
    {
        _input.asset.Enable();
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OffMove;
        _input.Player.Jump.performed += OnJump;
        _input.Player.Reverse.performed += OnReverse;
        _input.Player.ShowReverse.performed += OnShowReverse;
        _input.Player.Restart.performed += _ => { };
        _input.Player.Grab.performed += OnInteract;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OffMove;
        _input.Player.Jump.performed -= OnJump;
        _input.Player.Reverse.performed -= OnReverse;
        _input.Player.ShowReverse.performed -= OnShowReverse;
        _input.Player.Grab.performed -= OnInteract;
        _input.asset.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!_reverseView.IsPlayerView) return;

        _status.InputAxis.Value = ctx.ReadValue<Vector2>();

        if (!_status.IsGrab)
        {
            FlipXCheck();
        }
        
        GrabMoveAnimaiton();
    }

    private void OffMove(InputAction.CallbackContext ctx)
    {
        _status.InputAxis.Value = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_status.IsJumping || _status.IsFalling || !_reverseView.IsPlayerView) return;

        _anim.SetBool("Jump", true);
        _movement.ChangeJumpState(_movement.Jumping);
    }

    // 플레이어 반전 기능 Z키
    private void OnReverse(InputAction.CallbackContext ctx)
    {
        _reverseObjectScript.OnReverseGround();
        if (_status.IsJumping || _status.IsFalling || !_reverseObjectScript.CanReverse || !_reverseObjectScript.OnGround) return;
        if (!_status.IsReverse)
        {
            _anim.SetBool("Reverse", true);
        }
        else
        {
            _anim.SetBool("Reverse", false);
        }
        _status.IsReverse = !_status.IsReverse; 
        _reverse.Reverse();
        if (!_reverseView.IsPlayerView) _reverseView.ChangeReverseView();
        if (_status.GrabbedObject != null)
        {
            IReversable rv = _status.GrabbedObject as IReversable;
            rv?.Reverse();
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_status.IsJumping || _status.IsFalling) return;
        TryInteractObject();
    }

    // 카메라 시점 전환 X키
    private void OnShowReverse(InputAction.CallbackContext ctx)
    {
        _reverseObjectScript.OnReverseGround();
        if (_status.IsJumping || _status.IsFalling) return;
        if (_status.InputAxis.Value != Vector2.zero) _status.InputAxis.Value = Vector2.zero;
        _reverseView.ChangeReverseView();
    }

    private void SetDirection(Vector2 dir)
    {
        if (_status.IsGrab) return;
        if (dir.x > 0)
        {
            _status.IsRight = true;
            if (_grabPoint.localPosition.x < 0)
                _grabPoint.localPosition = new Vector2(_grabPoint.localPosition.x * -1, _grabPoint.localPosition.y);
        }
        else if (dir.x < 0)
        {
            _status.IsRight = false;
            if (_grabPoint.localPosition.x > 0)
                _grabPoint.localPosition = new Vector2(_grabPoint.localPosition.x * -1, _grabPoint.localPosition.y);
        }
    }

    private void TryInteractObject()
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
            _rightObject = GrabObjectPositionCheck();
            _anim.SetBool("Grab", true);
        }
    }

    public void OffGrab()
    {
        _status.GrabbedObject = null;
        _status.IsGrab = false;
        FlipXCheck();
        _anim.SetBool("Grab", false);
    }

    public void Respawn() //r키 눌렸을 때.
    {
        if (_isRespawning) return;
        _isRespawning = true;
        
        bool _spawnReverse = _spawnPos.y < 0;
        _movement._rigidbody.gravityScale = _status.GravityScale * (_spawnReverse ? -1 : 1);
        transform.localScale = new Vector2(transform.localScale.x * 1f, Mathf.Abs(transform.localScale.y) * (_spawnReverse ? -1 : 1));
        _anim.SetBool("Reverse", _spawnReverse);

        _status.IsReverse = _spawnReverse;
        transform.position = _spawnPos;

        _status.InputAxis.Value = Vector2.zero;
        
        _isRespawning = false;
    }

    private bool GrabObjectPositionCheck()
    {
        if (_renderer.flipX) return false;
        return true;
    }

    private void GrabMoveAnimaiton()
    {
        if (!_status.IsGrab) return;

        if (_rightObject)
        {
            if (_status.InputAxis.Value.x < 0f) _anim.SetBool("BackWalk", true);
            else _anim.SetBool("BackWalk", false);
        }
        else
        {
            if (_status.InputAxis.Value.x > 0f) _anim.SetBool("BackWalk", true);
            else _anim.SetBool("BackWalk", false);
        }
    }

    private void FlipXCheck()
    {
        if(_status.InputAxis.Value.x < 0)
            _renderer.flipX = true;
        else if (_status.InputAxis.Value.x > 0)
            _renderer.flipX = false; 
    }
}