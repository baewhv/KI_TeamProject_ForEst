using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CutScene;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneManager : SingletonMonoBehaviour<CutSceneManager>
{
    private Dictionary<string, ScenarioSO> Scenarios = new();
    [SerializeField] private ScenarioSO testSO;

    public Queue<ScenarioSO> ScenarioQueue { get; private set; } = new();

    // 컷씬용 오브젝트
    private GameObject _cutSceneObjects;
    
    private GameObject _cutSceneTriggerPrefab;

    private Camera mainCamera;
    public CinemachineCamera CutsceneCamera { get; private set; }
    private LayerMask cutsceneMask;
    private LayerMask beforeMask;

    public CutsceneUIController CinemaUI { get; private set; }

    // 비디오 플레이어
    public VideoPlayer VideoPlayer { get; private set; }

    // 연출 플레이어
    public CharacterCutsceneController Player { get; private set; }

    // 연출 NPC 시드
    public CharacterCutsceneController Seed { get; private set; }

    // 연출 NPC 시드콩
    public CharacterCutsceneController Seed_B { get; private set; }

    public GameObject EmptyObject { get; private set; }

    public ObserveValue<bool> IsPlayCutscene = new();
    [SerializeField] private bool UseTestSO = false;


    //SO에서 현재 실행중인 액션.
    private List<BaseAction> CurrentActions = new();

    private int CurrentActionsIndex;

    private PlayerController pc;

    private HashSet<int> cutsceneHash = new();

    private Dictionary<int, GameObject> CutsceneObject = new ();

    protected override void Awake()
    {
        base.Awake();
        InitPrefabs();
    }

    private void Start()
    {
        if (Scenarios.Count == 0)
        {
            if(!UseTestSO)
                LoadScenario(SceneManager.GetActiveScene().name);
            else
                Scenarios["Start"] = testSO;
            EnqueueCutscene("Start");
            Dialogue.Instance.CreateTextBox();
        }
    }

    private void InitPrefabs()
    {
        _cutSceneObjects = new GameObject("CutsceneObject");
        beforeMask = Resources.Load<LayerMaskSO>("Util/DefaultCameraMask").layerMask;
        cutsceneMask = Resources.Load<LayerMaskSO>("Util/CutsceneMask").layerMask;
        _cutSceneTriggerPrefab = Resources.Load<GameObject>("Prefab/Cutscene/CutsceneTrigger");

        GameObject videoGo = new GameObject("CutsceneVideoPlayer");
        videoGo.transform.SetParent(transform);
        VideoPlayer = videoGo.AddComponent<VideoPlayer>();
        VideoPlayer.playOnAwake = false;
        VideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;

        GameObject go = Resources.Load<GameObject>("Prefab/Cutscene/P_Est_Cutscene");
        pc = FindAnyObjectByType<PlayerController>();
        Player = Instantiate(go, transform).GetComponent<CharacterCutsceneController>();
            Player.Init(pc ? pc.GetStatus : null);
        go = Resources.Load<GameObject>("Prefab/Cutscene/CinemachineCamera");
        CutsceneCamera = Instantiate(go, transform).GetComponent<CinemachineCamera>();
        go = Resources.Load<GameObject>("Prefab/Cutscene/CutSceneCanvas");
        CinemaUI = Instantiate(go, transform).GetComponent<CutsceneUIController>();
        go = Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_Cutscene");
        Seed = Instantiate(go, transform).GetComponent<CharacterCutsceneController>();
        Seed.Init(null);
        go = Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_B_Cutscene");
        Seed_B = Instantiate(go, transform).GetComponent<CharacterCutsceneController>();
        Seed_B.Init(null);
        EmptyObject = new GameObject("EmptyObject");
        EmptyObject.transform.SetParent(transform);
    }

    //스테이지 명을 기준으로 시나리오를 호출하도록...
    public void LoadScenario(string stageName)
    {
        ScenarioSO[] datas = Resources.LoadAll<ScenarioSO>($"Scenario/{stageName}");
        foreach (var d in datas)
        {
            string name;
            switch (d.name[d.name.Length - 1])
            {
                case 'S':
                    name = "Start";
                    break;
                case 'E':
                    name = "End";
                    break;
                default:
                    name = d.name;
                    Debug.Log(name);
                    break;
            }
            
            Scenarios[name] = d;
            
            CreateTrigger(d.Triggers);
        }
        pc = FindAnyObjectByType<PlayerController>();
        Camera.main.cullingMask = beforeMask; //hardcoding
    }

    private void CreateTrigger(List<CutsceneTriggerData> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if(!_cutSceneObjects)
                _cutSceneObjects = new GameObject("CutsceneObject");
            CutsceneTrigger trigger = Instantiate(_cutSceneTriggerPrefab, _cutSceneObjects.transform).GetComponent<CutsceneTrigger>();
            trigger.Init(data[i]);
        }
    }

    public void UnLoadScenario()
    {
        foreach (GameObject obj in CutsceneObject.Values)
        {
            if(obj)
                Destroy(obj);
        }
        CutsceneObject.Clear();
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
            mainCamera.cullingMask = cutsceneMask;
        }
        SetCharacter(Player, pc.gameObject, ScenarioQueue.Peek().PlayerData);
        SetCharacter(Seed, null, ScenarioQueue.Peek().SeedData);
        SetCharacter(Seed_B, null, ScenarioQueue.Peek().SeedBData);
        SetCamera(ScenarioQueue.Peek().CameraData);
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

    public void EnqueueCutscene(string cutSceneName)
    {
        CurrentActionsIndex = 0;
        if (Scenarios.ContainsKey(cutSceneName))
        {
            ScenarioQueue.Enqueue(Scenarios[cutSceneName]);
            if(!IsPlayCutscene.Value)
                PlayScenarioQueue();
        }
    }

    private void PlayScenarioQueue()
    {
        CurrentActionsIndex = 0;
        if (ScenarioQueue.Count == 0) return; 
        if (!mainCamera)
            mainCamera = Camera.main;
        if (!pc)
            pc = FindAnyObjectByType<PlayerController>();
        EnableCutsceneMode();
        IsPlayCutscene.Value = true;
        SetNextCut();
    }

    public void EndCutscene()
    {
        if (ScenarioQueue.Count == 0)
        {
            IsPlayCutscene.Value = false;
            DisableCutsceneMode();
            return;
        }
        PlayScenarioQueue();
    }


    private void SetNextCut()
    {
        CurrentActions.Clear();
        if (ScenarioQueue.Count == 0 || CurrentActionsIndex >= ScenarioQueue.Peek().ActionList.Count)
        {
            ScenarioQueue.Dequeue();
            EndCutscene();
            return;
        }

        while (true)
        {
            BaseAction curAction = ScenarioQueue.Peek().ActionList[CurrentActionsIndex];
            curAction.ActionNum = CurrentActionsIndex;
            CurrentActions.Add(curAction); //현재 액션 추가
            cutsceneHash.Add(CurrentActionsIndex);
            CurrentActionsIndex++; //인덱스 추가

            if (CurrentActionsIndex >= ScenarioQueue.Peek().ActionList.Count ||
                curAction.NextType != ENextActionType.Together)
                break;
        }

        PlayCuts();
    }

    private void PlayCuts()
    {
        for (int i = 0; i < CurrentActions.Count; i++)
        {
            StartCoroutine(CurrentActions[i].PlayActionRoutine());
        }
    }

    public void EndAction(int num)
    {
        Debug.Log($"종료 호출 {num}");
        if (cutsceneHash.Contains(num))
        {
            cutsceneHash.Remove(num);
            if(cutsceneHash.Count <= 0)
                SetNextCut();
        }
        else
        {
            Debug.Log($"재생중이지 않은 액션의 종료 호출 {num}");
        }
    }

    public void SetCharacter(CharacterCutsceneController cutSceneObj, GameObject inGameObj, CharacterCutsceneData data)
    {
        if (inGameObj && data.GetInGamePosition)
        {
            cutSceneObj.SetPosition(inGameObj.transform.position);
        }
        else
        {
            cutSceneObj.SetPosition(data.position);
        }

        cutSceneObj.SetDirection(data.isRight);
        StartCoroutine(cutSceneObj.Fader(true, 0));
        cutSceneObj.gameObject.SetActive(data.ShowCharacter);
        cutSceneObj.ResetAnimation();
    }

    public void SetCamera(CameraCutsceneData data)
    {
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        brain.DefaultBlend.Time = 0;
        if (data.followTarget)
        {
            switch (data.target)
            {
                case ESelectedCharacter.Est:
                    CutsceneCamera.Follow = Player.transform;
                    break;
                case ESelectedCharacter.Seed:
                    CutsceneCamera.Follow = Seed.transform;
                    break;
                case ESelectedCharacter.Seed_B:
                    CutsceneCamera.Follow = Seed_B.transform;
                    break;
            }
        }
        else
        {
            CutsceneCamera.Follow = null;
            CutsceneCamera.transform.position = new Vector3(data.position.x, data.position.y, -10.0f);
        }

        CutsceneCamera.Lens.OrthographicSize = data.zoom < 1 ? 1 : data.zoom;
        StartCoroutine(ResetCameraBlend());
    }

    public CharacterCutsceneController GetCharacter(ESelectedCharacter character)
    {
        switch (character)
        {
            case ESelectedCharacter.Est:
                return Player;
            case ESelectedCharacter.Seed:
                return Seed;
            case ESelectedCharacter.Seed_B:
                return Seed_B;
            default:
                return null;
        }
    }

    private IEnumerator ResetCameraBlend()
    {
        yield return new WaitForNextFrameUnit();
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        brain.DefaultBlend.Time = 2;
    }

    public void CreateObject(int objectNumber, Vector2 position)
    {
        if (!CutsceneObject.ContainsKey(objectNumber))
        {
            CutsceneObject[objectNumber] =
                Instantiate(ScenarioQueue.Peek().CutsceneObject[objectNumber], _cutSceneObjects.transform);
            CutsceneObject[objectNumber].transform.position = position;
        }
    }
}