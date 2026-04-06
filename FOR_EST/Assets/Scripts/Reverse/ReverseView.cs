using Unity.Cinemachine;
using UnityEngine;

public class ReverseView : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _reverseIcon;

    private GameObject _reverseObject;
    private CinemachineCamera _camera;
    private Animator _anim;
    private PlayerController _playerController;
    private PlayerReverseObject _playerReverseObject;
    private SpriteRenderer _renderer;
    
    public bool IsPlayerView { get; private set; }

    private void Awake()
    {
        IsPlayerView = true;
        _camera = GetComponent<CinemachineCamera>();
        if (_reverseIcon != null) CreateReverseIcon();
        if (_reverseIcon != null) _anim = _reverseIcon.GetComponent<Animator>();
        _playerController = _player.GetComponent<PlayerController>();
    }
    
    public void ChangeReverseView()
    {
        if (_reverseObject == null)
        {
            _reverseObject = GameObject.FindGameObjectWithTag("Reverse");
            _playerReverseObject = _reverseObject.GetComponent<PlayerReverseObject>();
        }
        if (_reverseIcon == null || _reverseObject == null) return;
        
        if (IsPlayerView)
        {
            _camera.Follow = _reverseObject.transform;
            _camera.LookAt = _reverseObject.transform;
            
            _reverseIcon.transform.position = _reverseObject.transform.position;
            
            if (!_playerController.IsReverse)
            {
                _anim.SetBool("Reverse", true);
                _renderer.flipY = true;
            }
            else
            {
                _anim.SetBool("Reverse", false);
            }

            if (!_playerReverseObject.CanReverse || !_playerReverseObject.OnGround)
            {
                _anim.SetBool("NoReverse", true);
            }
            else
            {
                _anim.SetBool("NoReverse", false);
            }
            
            _renderer.enabled = true;
        }
        else
        {
            _camera.Follow = _player.transform;
            _camera.LookAt = _player.transform;
            _renderer.flipY = false;
            _renderer.enabled = false;
        }
        
        IsPlayerView = !IsPlayerView;
    }
    
    private void CreateReverseIcon()
    {
        _reverseIcon = Instantiate(_reverseIcon);
        _renderer = _reverseIcon.GetComponent<SpriteRenderer>();
        _renderer.enabled = false;
    }
}
