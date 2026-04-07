using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedBean2 : BaseSeedBean
{
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        base.Init();
        _anim.SetBool("Head", RandomSetBool());
    }
}
