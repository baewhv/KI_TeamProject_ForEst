using Unity.Cinemachine;
using UnityEngine;

public class ReverseView : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private GameObject _reverseIcon;

    private GameObject _reverseObject;
    private CinemachineCamera _camera;

    public bool IsPlayerView { get; private set; }

    private void Awake()
    {
        IsPlayerView = true;
        _camera = GetComponent<CinemachineCamera>();
        if (_reverseIcon != null) CreateReverseIcon();
    }
    
    public void ChangeReverseView()
    {
        if (_reverseObject == null) _reverseObject = GameObject.FindGameObjectWithTag("Reverse");
        if (_reverseIcon == null || _reverseObject == null) return;
        
        if (IsPlayerView)
        {
            _camera.Follow = _reverseObject.transform;
            _camera.LookAt = _reverseObject.transform;
            
            _reverseIcon.transform.position = _reverseObject.transform.position;
            _reverseIcon.SetActive(true);
        }
        else
        {
            _camera.Follow = _player;
            _camera.LookAt = _player;
            _reverseIcon.SetActive(false);
        }
        
        IsPlayerView = !IsPlayerView;
    }
    
    private void CreateReverseIcon()
    {
        _reverseIcon = Instantiate(_reverseIcon);
        _reverseIcon.SetActive(false);
    }
}
