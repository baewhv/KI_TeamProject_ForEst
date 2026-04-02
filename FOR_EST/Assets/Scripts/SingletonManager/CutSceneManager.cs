using System;
using System.Collections.Generic;
using CutScene;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CutSceneManager : SingletonMonoBehaviour<CutSceneManager>
{
    private Dictionary<string, ScenarioSO> Scenarios = new();
    [SerializeField] private ScenarioSO testSO;
    private ScenarioSO CurrentScenario;
    // 컷씬용 오브젝트
    private GameObject CutSceneObjects;

    private Camera mainCamera;
    public CinemachineCamera CutsceneCamera { get; private set; }
    [SerializeField] private LayerMask CutsceneMask;
    private LayerMask beforeMask;
    public CutsceneUIController CinemaUI { get; private set; }

    // 연출 플레이어
    public PlayerCutSceneController Player { get; private set; }

    // 연출 NPC 시드
    public GameObject Seed { get; private set; }

    // 연출 NPC 시드콩
    public GameObject Seed_B { get; private set; }



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
    }

    private void InitPrefabs()
    {
        CutSceneObjects = new GameObject("CutsceneObject");
        GameObject go = Resources.Load<GameObject>("Prefab/Cutscene/P_Est_Cutscene");
        pc = FindAnyObjectByType<PlayerController>();
        Player = Instantiate(go, pc.transform.position, pc.transform.rotation, transform).GetComponent<PlayerCutSceneController>();
        Player.Init(pc.GetStatus);
        go = Resources.Load<GameObject>("Prefab/Cutscene/CinemachineCamera");
        CutsceneCamera = Instantiate(go, transform).GetComponent<CinemachineCamera>();
        go = Resources.Load<GameObject>("Prefab/Cutscene/CutSceneCanvas");
        CinemaUI = Instantiate(go, transform).GetComponent<CutsceneUIController>();
        Seed = Instantiate(Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_Cutscene"), new Vector2(-0.73f, 1.12f), Quaternion.identity);
        Seed_B = Instantiate(Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_B_Cutscene"), new Vector2(-5.21f, 0.72f), Quaternion.identity);
    }

    private void Update()
    {
        foreach (var act in CurrentActions)
        {
            act.Update();
        }
    }

    private void Start()
    {
        LoadScenario("StageT");
        PlayCutscene("Start");
    }

    //스테이지 명을 기준으로 시나리오를 호출하도록...
    public void LoadScenario(string stageName)
    {
        ScenarioSO[] datas = Resources.LoadAll<ScenarioSO>($"Scenario/{stageName}");
        foreach (var d in datas)
        {
            string name;
            switch (d.name[d.name.Length-1])
            {
                case 'S':
                    name = "Start";
                    break;
                case 'E':
                    name = "End";
                    break;
                default:
                    name = d.name.Remove(stageName.Length);
                    break;
            }

            Scenarios[name] = d;
        }
    }

    public void UnLoadScenario()
    {
        Scenarios.Clear();
    }



    /// <summary>
    /// 컷씬 시작 시 세팅해야할 것들
    /// </summary>
    private void EnableCutsceneMode()
    {
        pc.gameObject.SetActive(false); //입력 제한 용
        CutsceneCamera.Priority = 11; //
        if (mainCamera)
        {
            beforeMask = mainCamera.cullingMask;
            mainCamera.cullingMask = CutsceneMask;
        }

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
        if(!mainCamera)
            mainCamera = Camera.main;
        if (Scenarios.Count == 0)
            CurrentScenario = testSO;
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

    public void SetCharacter(GameObject cutSceneObj, GameObject inGameObj, CharacterCutsceneData data)
    {
        ICutsceneObject obj = cutSceneObj.GetComponent<ICutsceneObject>();
        if (data.GetInGamePosition)
        {
            obj?.SetPosition(inGameObj.transform.position);
            //cutSceneObj.SetDirection(); // 실제 플레이어의 방향 가져오기.
        }
        else
        {
            obj?.SetPosition(data.position);
            obj?.SetDirection(data.isRight);
        }
        cutSceneObj.gameObject.SetActive(data.ShowCharacter);
    }


}