using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 조작 부분
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStatus _status = new PlayerStatus();   
    private PlayerMovement _movement;
    private PlayerReverse _reverse;
    
    private UserInput _input;
    private GameObject _grabObject;

    private void Awake()
    {
        _input = new UserInput();
        _movement = GetComponent<PlayerMovement>(); 
        _movement.Init(_status);
        _reverse = GetComponent<PlayerReverse>();
    }

    private void OnEnable()
    {
        _input.asset.Enable();
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OffMove;
        _input.Player.Jump.performed += OnJump;
        _input.Player.Reverse.performed += OnReverse;
        _input.Player.ShowReverse.performed += _ => { };
        _input.Player.Restart.performed += _ => { };
    }

    private void OnDisable()
    {
        _input.asset.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _status.InputAxis = ctx.ReadValue<Vector2>();
    }

    private void OffMove(InputAction.CallbackContext ctx)
    {
        _status.InputAxis = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_status.IsJumping || _status.IsFalling) return;
        _movement.ChangeJumpState(_movement.Jumping);
    }

    private void OnReverse(InputAction.CallbackContext ctx)
    {
        _reverse.Reverse();
    }
}
