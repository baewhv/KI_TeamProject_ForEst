using System;
using System.Collections.Generic;
using CutScene;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CutSceneManager : SingletonMonoBehaviour<CutSceneManager>
{
    private StateMachine CutSceneState;
    [SerializeField] private GameObject _cinemaCameraPrefab;
    [SerializeField] private GameObject _cinemaCanvasPrefab;
    public CinemachineCamera _cinemaCamera { get; private set; }
    private Dictionary<string, ScenarioSO> Scenarios = new ();

    [SerializeField] private ScenarioSO testSO;
    private GameObject CutSceneObjects;


    private ScenarioSO CurrentScenario;
    //SO에서 현재 실행중인 액션.
    private List<BaseAction> CurrentActions = new ();
    //SO에서 현재 재생중인 액션 개수.
    private int _currentActionsCount;
    private int CurrentActionsIndex;
    
    
    protected override void Awake(){
        base.Awake();
        CutSceneObjects = new GameObject();
        SetCamera();
        //특정 위치의 시나리오 모두 로드.
        
        //임시 추가
        if(Scenarios.Count == 0)
            Scenarios["start"] = testSO;
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

    private void SetCamera()
    {
        if(_cinemaCameraPrefab)
            _cinemaCamera = Instantiate(_cinemaCameraPrefab, CutSceneObjects.transform).GetComponent<CinemachineCamera>();
    }


    public void PlayCutscene(string cutSceneName)
    {
        if (CurrentScenario != null) return; //현재 실행중 시나리오가 있는지
        //컷씬 so 세팅.
        CurrentActionsIndex = 0;
        if (Scenarios.ContainsKey(cutSceneName))
        {
            CurrentScenario = Scenarios[cutSceneName];
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
            CurrentActionsIndex++;//인덱스 추가
            _currentActionsCount++;
            if (CurrentActionsIndex >= CurrentScenario.ActionList.Count || curAction.NextType != ENextActionType.Together) 
                break;
        }
        PlayCuts();
    }

    private void PlayCuts()
    {
        foreach (var act in CurrentActions)
        {
            act.PlayAction();
            StartCoroutine(act.PlayActionRoutine());
        }
    }

    public void EndAction()
    {
        _currentActionsCount--;
        if(_currentActionsCount <= 0)
            SetNextCut();
    }
}
