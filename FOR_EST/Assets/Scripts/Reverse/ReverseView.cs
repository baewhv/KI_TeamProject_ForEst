using Unity.Cinemachine;
using UnityEngine;

public class ReverseView : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _reverseObject;
    
    [SerializeField] private GameObject _reverseIcon;
    
    private CinemachineCamera _camera;

    private bool _isPlayerView;

    private void Awake()
    {
        _isPlayerView = true;
        _camera = GetComponent<CinemachineCamera>();
        if (_reverseObject != null) _reverseIcon.SetActive(false);
    }
    
    public void ChangeReverseView()
    {
        if (_reverseIcon == null) return;
        
        if (_isPlayerView)
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
        
        _isPlayerView = !_isPlayerView;
    }
}
