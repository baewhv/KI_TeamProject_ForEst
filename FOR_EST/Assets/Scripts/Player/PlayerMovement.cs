using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStatus _status;

    public Rigidbody2D _rigidbody { get; private set; }
    private BoxCollider2D _collider;

    [SerializeField] private ContactFilter2D GroundFilter;

    private StateMachine _jumpStateMachine;
    public JumpStandbyState JumpStandby { get; private set; }
    public JumpingState Jumping { get; private set; }
    public FallingState Falling { get; private set; }
    public LandingState Landing { get; private set; }

    public Animator Anim { get; private set; }

    private float _walkAnimSpeed;

    [field: SerializeField] public ObserveValue<EJumpState> JumpState { get; set; }

    private LayerMask _jumpFallingLayer;

    public void Init(PlayerStatus status)
    {
        _status = status;
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _jumpStateMachine = new StateMachine();
        JumpState = new ObserveValue<EJumpState>();

        JumpStandby = new JumpStandbyState(_status, this);
        Jumping = new JumpingState(_status, this);
        Falling = new FallingState(_status, this);
        Landing = new LandingState(_status, this);

        _jumpStateMachine.ChangeState(JumpStandby);
        Anim = GetComponentInChildren<Animator>();
        _jumpFallingLayer = LayerMask.GetMask("Ground", "Obstacle", "Happy", "Sad");
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
            Anim.SetFloat("MoveSpeed", _walkAnimSpeed);
        }
        else
        {
            _walkAnimSpeed = Mathf.Lerp(_walkAnimSpeed, 0f, 0.4f);
            if (_walkAnimSpeed < 0.1) _walkAnimSpeed = 0f;
            Anim.SetFloat("MoveSpeed", _walkAnimSpeed);
        }
    }

    public void ChangeJumpState(IState state)
    {
        _jumpStateMachine.ChangeState(state);
    }

    public bool IsGround(bool reverseCheck = false)
    {
        Vector2 origin = _rigidbody.position + new Vector2(0, _collider.size.y * 0.5f * (_status.IsReverse ? 1 : -1));
        if (reverseCheck)
            origin = new Vector2(origin.x, -origin.y);
        Vector2 boxSize = new Vector2(_collider.size.x, 0.2f);
        GizmoHelper.Instance.SetBox(name, origin, boxSize, reverseCheck ? Color.blue : Color.red);
       List<RaycastHit2D> hits = new List<RaycastHit2D>();
       if (Physics2D.BoxCast(origin, boxSize, 0, Vector2.zero, GroundFilter, hits, 0) > 0)
        {
            if(reverseCheck) Debug.Log("Check");
            return true;
        }
        return false;
    }

    public bool LandingReady()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            (_status.IsReverse
                ? transform.position + new Vector3(0f, 1f, 0f)
                : transform.position - new Vector3(0f, 1f, 0f))
            , 0.4f
            , (_status.IsReverse ? Vector2.up : Vector2.down)
            , 3f
            , _jumpFallingLayer);

        if (hit) return true;
        return false;
    }
}