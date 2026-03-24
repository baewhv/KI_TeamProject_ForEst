using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 전반적인 클리어 조건
/// 열매가 전부 목적 포인트에 도달하여 충족되는지 체크
/// 필드에 존재하는 열매 개수가 0이 되면 클리어 조건 활성화
/// </summary>
/// 
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    
    public int HappyFruitCount { private get; set; }
    public int SadFruitCount { private get; set; }

    private LayerMask _happyMask;
    private LayerMask _sadMask;

    public bool IsClear { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        Init();
    }
    
    private void Init()
    {
        _happyMask = LayerMask.GetMask("HappyFruit");
        _sadMask = LayerMask.GetMask("SadFruit");
    }

    // SceneManagement 스크립트에서 Scene 전환 시
    // 클리어 조건을 1프레임 이후에 체크하도록 구현하기 위해 코루틴 사용
    // 즉시 체크하면 Awake 단계에서 오브젝트가 생성되지 않은 상태에서 검사하는 경우가 발생
    public void OnSceneLoadedCheck()
    {
        StartCoroutine(DelayCheck());
    }

    private IEnumerator DelayCheck()
    {
        yield return null;
        
        CheckFruitCount();
    }

    private void CheckFruitCount()
    {
        Collider2D[] hitHappy = Physics2D.OverlapBoxAll(transform.position, new Vector2(90, 57), 0f, _happyMask);

        foreach (Collider2D hit in hitHappy)
        {
            HappyFruitCount++;
        }
        
        Collider2D[] hitSad = Physics2D.OverlapBoxAll(transform.position, new Vector2(90, 57), 0f, _sadMask);

        foreach (Collider2D hit in hitSad)
        {
            SadFruitCount++;
        }
    }

    private void CheckClear()
    {
        if (HappyFruitCount != 0 || SadFruitCount != 0) return;

        IsClear = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(0, 0), new Vector2(90, 57));
    }
}
