using Unity.Cinemachine;
using UnityEngine;

public class ReverseView : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _reverseObject;
    [SerializeField] private GameObject _reverseIcon;

    private GameObject _createdIcon;
    
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
        if (_reverseIcon == null) return;
        
        if (IsPlayerView)
        {
            _camera.Follow = _reverseObject;
            _camera.LookAt = _reverseObject;
            
            _reverseIcon.transform.position = _reverseObject.position;
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
