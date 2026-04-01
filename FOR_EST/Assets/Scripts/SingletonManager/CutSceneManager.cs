using System;
using System.Collections.Generic;
using CutScene;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CutSceneManager : SingletonMonoBehaviour<CutSceneManager>
{
    private Dictionary<string, ScenarioSO> Scenarios = new();

    // 컷씬용 오브젝트
    private GameObject CutSceneObjects;

    private Camera mainCamera;
    public CinemachineCamera CutsceneCamera { get; private set; }
    [SerializeField] private LayerMask CutsceneMask;
    private LayerMask beforeMask;
    public CutsceneUIController CinemaUI { get; private set; }

    /// <summary>
    /// 연출 플레이어
    /// Layer : Cutscene_NPC
    /// Tag : Player
    /// </summary>
    public PlayerCutSceneController Player { get; private set; }

    /// <summary>
    /// 연출 NPC 시드
    /// Layer : Cutscene_NPC
    /// Tag : Seed
    /// </summary>
    public GameObject Seed { get; private set; }

    /// <summary>
    /// 연출 NPC 시드콩
    /// Layer : Cutscene_NPC
    /// Tag : Seed
    /// </summary>
    public GameObject Seed_B { get; private set; }


    [SerializeField] private ScenarioSO testSO;

    private ScenarioSO CurrentScenario;

    //SO에서 현재 실행중인 액션.
    private List<BaseAction> CurrentActions = new();

    //SO에서 현재 재생중인 액션 개수.
    private int _currentActionsCount;
    private int CurrentActionsIndex;

    private PlayerController pc;


    protected override void Awake()
    {
        base.Awake();

        InitPrefabs();

        mainCamera = Camera.main;
        //임시 추가
        if (Scenarios.Count == 0)
            Scenarios["start"] = testSO;
    }

    private void InitPrefabs()
    {
        CutSceneObjects = new GameObject("CutsceneObject");
        GameObject go = Resources.Load<GameObject>("Prefab/Cutscene/P_Est_Cutscene");
        pc = FindAnyObjectByType<PlayerController>();
        Player = Instantiate(go, pc.transform.position, pc.transform.rotation,
            CutSceneObjects.transform).GetComponent<PlayerCutSceneController>();
        Player.Init(pc.GetStatus);
        //Player.gameObject.SetActive(false);
        go = Resources.Load<GameObject>("Prefab/Cutscene/CinemachineCamera");
        CutsceneCamera = Instantiate(go, CutSceneObjects.transform).GetComponent<CinemachineCamera>();
        go = Resources.Load<GameObject>("Prefab/Cutscene/CutSceneCanvas");
        CinemaUI = Instantiate(go, CutSceneObjects.transform).GetComponent<CutsceneUIController>();
        Seed = Instantiate(Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_Cutscene"), new Vector2(-0.73f, 1.12f),
            Quaternion.identity);
        Seed_B = Instantiate(Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_B_Cutscene"), new Vector2(-5.21f, 0.72f),
            Quaternion.identity);
    }

    private void Start()
    {
        PlayCutscene("start");
    }

    private void Update()
    {
        foreach (var act in CurrentActions)
        {
            act.Update();
        }
    }



    /// <summary>
    /// 컷씬 시작 시 세팅해야할 것들
    /// </summary>
    private void EnableCutsceneMode()
    {
        pc.gameObject.SetActive(false);
        CutsceneCamera.Priority = 11; //
        if (mainCamera)
        {
            beforeMask = mainCamera.cullingMask;
            mainCamera.cullingMask = CutsceneMask;
        }

        //연출 초기 설정
        //if(CurrentScenario.Player.bGetInGamePosition)  
        // Player.transform =  realPlayer.transform;
        //else
        //Player.SetPosition(CurrentScenario.Player.Position, true); // 강제이동  및 반전 체크
        //Player.trasnform.position = CurrentScenario.Player.Position;
    }

    private void DisableCutsceneMode()
    {
        if (mainCamera)
        {
            mainCamera.cullingMask = beforeMask;
        }
        CutsceneCamera.Priority = 9;
        //연출 종료 설정
        //if(CurrentScenario.Player.SetInGamePosition)
        //realPlayer.transform.position = Player.transform.position;
        pc.gameObject.SetActive(true);
    }
    
    public void PlayCutscene(string cutSceneName)
    {
        if (CurrentScenario != null) return; //현재 실행중 시나리오가 있는지
        //컷씬 so 세팅.
        CurrentActionsIndex = 0;
        if (Scenarios.ContainsKey(cutSceneName))
        {
            CurrentScenario = Scenarios[cutSceneName];
            EnableCutsceneMode();
            SetNextCut();
        }
        else
        {
            Debug.Log("실행할 시나리오 없음.");
        }
        
    }

    public void EndCutscene()
    {
        CurrentScenario = null;
        DisableCutsceneMode();
    }
    

    private void SetNextCut()
    {
        CurrentActions.Clear();
        if (!CurrentScenario || CurrentActionsIndex >= CurrentScenario.ActionList.Count)
        {
            EndCutscene();
            return;
        }

        while (true)
        {
            BaseAction curAction = CurrentScenario.ActionList[CurrentActionsIndex];
            Debug.Log($"{CurrentActionsIndex} : {curAction.GetType()}");
            CurrentActions.Add(curAction); //현재 액션 추가
            CurrentActionsIndex++; //인덱스 추가
            _currentActionsCount++;
            if (CurrentActionsIndex >= CurrentScenario.ActionList.Count ||
                curAction.NextType != ENextActionType.Together)
                break;
        }

        PlayCuts();
    }

    private void PlayCuts()
    {
        for (int i = 0; i < CurrentActions.Count; i++)
        {
            CurrentActions[i].InitAction();
            StartCoroutine(CurrentActions[i].PlayActionRoutine());
        }
    }

    public void EndAction()
    {
        _currentActionsCount--;
        if (_currentActionsCount <= 0)
            SetNextCut();
    }
}