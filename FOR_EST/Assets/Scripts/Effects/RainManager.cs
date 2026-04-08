using UnityEngine;

public class RainManager : MonoBehaviour
{
    [SerializeField] private GameObject _sadRain;
    [SerializeField] private GameObject _happyRain;
    [SerializeField] private GameObject _rainfloor;
    [SerializeField] private GameObject _raintile;

    private void Update()
    {
        if (GameManager.Instance.IsClear)
        { 
            _sadRain.SetActive(false);
            _happyRain.SetActive(false);
            _rainfloor.SetActive(false);
            _raintile.SetActive(false);
        }
    }
}
