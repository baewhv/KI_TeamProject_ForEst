using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 전반적인 클리어 조건
/// 열매가 전부 목적 포인트에 도달하여 충족되는지 체크
/// 필드에 존재하는 열매 개수가 0이 되면 클리어 조건 활성화
/// </summary>
/// 
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private GameObject _player;
    private PlayerStartPoint _startPoint;
    
    public int FruitCount { get; set; }
    
    private LayerMask _fruitMask;
    private LayerMask _playerMask;
    private PlayerController _playerController;

    protected override void Awake()
    {
        base.Awake();
        
        Init();
    }
    
    private void Init()
    {
        _fruitMask = LayerMask.GetMask("Happy", "Sad");
    }

    // SceneManagement 스크립트에서 Scene 전환 시
    // 클리어 조건을 1프레임 이후에 체크하도록 구현하기 위해 코루틴 사용
    // 즉시 체크하면 Awake 단계에서 오브젝트가 생성되지 않은 상태에서 검사하는 경우가 발생
    public void OnSceneLoadedCheck()
    {
        if (SceneManagement.Instance.CurrentSceneName == "TitleScene_SHY") return;
        StartCoroutine(DelayCheck());
    }

    private IEnumerator DelayCheck()
    {
        yield return null;

        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        if (_playerController == null) _playerController = _player.GetComponent<PlayerController>();
        
        CutSceneManager.Instance.LoadScenario(SceneManagement.Instance.CurrentSceneName);
        CutSceneManager.Instance.EnqueueCutscene("Start");
        
        CheckFruitCount();
    }

    private IEnumerator ClearDelayRoutine()
    {
        yield return YieldContainer.WaitForSeconds(2f);
        PlayEndCutscene();
    }

    private void CheckFruitCount()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(90, 80), 0f, _fruitMask);

        foreach (Collider2D hit in hits)
        {
            FruitCount++;
        }
    }
    
    public void CheckClear()
    {
        if (FruitCount != 0) return;

        Debug.Log("클리어!");
        StartCoroutine(ClearDelayRoutine());
    }

    private void PlayEndCutscene()
    {
        CutSceneManager.Instance.EnqueueCutscene("End");
        CutSceneManager.Instance.IsPlayCutscene.AddListener(EndCutsceneHandler);
    }

    private void EndCutsceneHandler(bool trigger)
    {
        if (!trigger)
        {
            CutSceneManager.Instance.IsPlayCutscene.RemoveListener(EndCutsceneHandler);
            SceneManagement.Instance.LoadNextScene();
        }
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(0, 0), new Vector2(90, 80));
    }
}
