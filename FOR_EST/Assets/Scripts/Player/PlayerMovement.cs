using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStatus _status;
    private PlayerController _controller;

    public Rigidbody2D _rigidbody { get; private set; }
    private BoxCollider2D _collider; 
    
    [SerializeField] private ContactFilter2D GroundFilter;

    private StateMachine _jumpStateMachine;
    public JumpStandbyState JumpStandby { get; private set; }
    public JumpingState Jumping { get; private set; }
    public FallingState Falling { get; private set; }
    public LandingState Landing { get; private set; }
    
    private Animator _anim;

    private float _walkAnimSpeed;

    [field:SerializeField] public ObserveValue<EJumpState> JumpState { get; set; }
    
    public void Init(PlayerStatus status)
    {
        _status = status;
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _controller = GetComponent<PlayerController>();
        _jumpStateMachine = new StateMachine();
        JumpState = new ObserveValue<EJumpState>();
        
        JumpStandby = new JumpStandbyState(_status, this);
        Jumping = new JumpingState(_status, this);
        Falling = new FallingState(_status, this);
        Landing = new LandingState(_status, this);

        _jumpStateMachine.ChangeState(JumpStandby);
        _anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        _jumpStateMachine.Update();
    }

    private void FixedUpdate()
    {
        //좌우 입력
        _rigidbody.linearVelocityX = _status.InputAxis.Value.x * _status.MoveSpeed * Time.fixedDeltaTime;
        
        
        if (Mathf.Abs(_status.InputAxis.Value.x) > 0)
        {
            _walkAnimSpeed = 1f;
            _anim.SetFloat("MoveSpeed", _walkAnimSpeed);
        }
        else
        {
            _walkAnimSpeed = Mathf.Lerp(_walkAnimSpeed, 0f, 0.4f);
            _anim.SetFloat("MoveSpeed", _walkAnimSpeed);
        }
    }

    public void ChangeJumpState(IState state)
    {
        _jumpStateMachine.ChangeState(state);
    }

    public bool IsGround()
    {
        Vector2 origin = _rigidbody.position + new Vector2(0, _collider.size.y * 0.5f * (_controller._isReverse ? 1 : -1));
        Vector2 boxSize = new Vector2(_collider.size.x, 0.2f);
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        //GizmoHelper.Instance.SetGizmos(gameObject.name, origin, origin + Vector2.down * _collider.size.y * 0.2f);
        if (Physics2D.BoxCast(origin,boxSize, 0, Vector2.zero, GroundFilter, hits,0) > 0)
            return true;
        return false;
    }

}