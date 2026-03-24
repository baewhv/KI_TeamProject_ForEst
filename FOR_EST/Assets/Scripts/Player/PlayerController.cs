using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 조작 부분
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStatus _status = new PlayerStatus();
    [SerializeField] private Transform _grabPoint;
    private PlayerMovement _movement;
    private PlayerReverse _reverse;
    private ReverseView _reverseView;
    
    private UserInput _input;
    private GameObject _grabObject;
    
    
    
    

    private void Awake()
    {
        _input = new UserInput();
        _movement = GetComponent<PlayerMovement>(); 
        _movement.Init(_status);
        _reverse = GetComponent<PlayerReverse>();
        _reverseView = GetComponentInChildren<ReverseView>();
        _status.InputAxis.AddListener(SetDirection);
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
        _status.InputAxis.Value = ctx.ReadValue<Vector2>();
    }

    private void OffMove(InputAction.CallbackContext ctx)
    {
        _status.InputAxis.Value = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_status.IsJumping || _status.IsFalling) return;
        _movement.ChangeJumpState(_movement.Jumping);
    }

    // 플레이어 반전 기능 Z키
    private void OnReverse(InputAction.CallbackContext ctx)
    {
        _reverse.Reverse();
        if (_status.GrabbedObject != null)
        {
            IReversable rv = _status.GrabbedObject as IReversable;
            rv?.Reverse();
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        TryInteractObject();
    }

    // 카메라 시점 전환 X키
    private void OnShowReverse(InputAction.CallbackContext ctx)
    {
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
            _status.GrabbedObject.OnStopP();
            OffGrab();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * (_status.IsRight ? 1 : -1), 0.5f, 1<<12);
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
}
