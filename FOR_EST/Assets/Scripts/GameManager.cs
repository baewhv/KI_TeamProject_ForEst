using System;
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

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        CheckClear();
    }

    private void Init()
    {
        _happyMask = LayerMask.GetMask("HappyFruit");
        _sadMask = LayerMask.GetMask("SadFruit");
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
