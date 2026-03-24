using Unity.Cinemachine;
using UnityEngine;

public class ReverseView : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _reverseObject;
    
    private CinemachineCamera _camera;

    private bool _isPlayerView;

    private void Awake()
    {
        _isPlayerView = true;
        _camera = GetComponent<CinemachineCamera>();
    }
    
    public void ChangeReverseView()
    {
        if (_isPlayerView)
        {
            _camera.Follow = _reverseObject;
            _camera.LookAt = _reverseObject;
        }
        else
        {
            _camera.Follow = _player;
            _camera.LookAt = _player;
        }
        
        _isPlayerView = !_isPlayerView;
    }
}
