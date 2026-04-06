using System.Collections;
using System.Collections.Generic;
using CutScene;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.Video;
using UnityEngine;

public class CutSceneManager : SingletonMonoBehaviour<CutSceneManager>
{
    private Dictionary<string, ScenarioSO> Scenarios = new();
    [SerializeField] private ScenarioSO testSO;

    public ScenarioSO CurrentScenario { get; private set; }

    // 컷씬용 오브젝트
    private GameObject CutSceneObjects;

    private Camera mainCamera;
    public CinemachineCamera CutsceneCamera { get; private set; }
    private LayerMask cutsceneMask;
    private LayerMask beforeMask;

    public CutsceneUIController CinemaUI { get; private set; }

    // 비디오 플레이어
    public VideoPlayer VideoPlayer { get; private set; }

    // 연출 플레이어
    public PlayerCutSceneController Player { get; private set; }

    // 연출 NPC 시드
    public PlayerCutSceneController Seed { get; private set; }

    // 연출 NPC 시드콩
    public PlayerCutSceneController Seed_B { get; private set; }

    public GameObject EmptyObject { get; private set; }

    public ObserveValue<bool> IsPlayCutscene = new();


    //SO에서 현재 실행중인 액션.
    private List<BaseAction> CurrentActions = new();

    //SO에서 현재 재생중인 액션 개수.
    private int _currentActionsCount;
    private int CurrentActionsIndex;

    private PlayerController pc;


    private HashSet<int> cutsceneHash = new();


    protected override void Awake()
    {
        base.Awake();
        InitPrefabs();
    }

    private void Start()
    {
        if (Scenarios.Count == 0)
        {
            Scenarios["test"] = testSO;
            PlayCutscene("test");
            Dialogue.Instance.CreateTextBox();
        }
    }

    private void InitPrefabs()
    {
        CutSceneObjects = new GameObject("CutsceneObject");
        beforeMask = Resources.Load<LayerMaskSO>("Util/DefaultCameraMask").layerMask;
        cutsceneMask = Resources.Load<LayerMaskSO>("Util/CutsceneMask").layerMask;

        GameObject videoGo = new GameObject("CutsceneVideoPlayer");
        videoGo.transform.SetParent(transform);
        VideoPlayer = videoGo.AddComponent<VideoPlayer>();
        VideoPlayer.playOnAwake = false;
        VideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;

        GameObject go = Resources.Load<GameObject>("Prefab/Cutscene/P_Est_Cutscene");
        pc = FindAnyObjectByType<PlayerController>();
        Player = Instantiate(go, transform).GetComponent<PlayerCutSceneController>();
        Player.Init(pc.GetStatus);
        go = Resources.Load<GameObject>("Prefab/Cutscene/CinemachineCamera");
        CutsceneCamera = Instantiate(go, transform).GetComponent<CinemachineCamera>();
        go = Resources.Load<GameObject>("Prefab/Cutscene/CutSceneCanvas");
        CinemaUI = Instantiate(go, transform).GetComponent<CutsceneUIController>();
        go = Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_Cutscene");
        Seed = Instantiate(go, transform).GetComponent<PlayerCutSceneController>();
        Seed.Init(null);
        go = Resources.Load<GameObject>("Prefab/Cutscene/N_Seed_B_Cutscene");
        Seed_B = Instantiate(go, transform).GetComponent<PlayerCutSceneController>();
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
                    name = d.name.Remove(stageName.Length);
                    break;
            }

            Scenarios[name] = d;
        }

        Camera.main.cullingMask = beforeMask; //hardcoding
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
            mainCamera.cullingMask = cutsceneMask;
        }

        SetCharacter(Player, pc.gameObject, CurrentScenario.PlayerData);
        SetCharacter(Seed, null, CurrentScenario.SeedData);
        SetCharacter(Seed_B, null, CurrentScenario.SeedBData);
        SetCamera(CurrentScenario.CameraData);
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
        if (!mainCamera)
            mainCamera = Camera.main;
        if (!pc)
            pc = FindAnyObjectByType<PlayerController>(); //Todo : PlayerCharacter 게임매니저에서 받아오기
        //if (!seed)                                        //Todo : Seed 게임 매니저에서 받아오기
        //seed = 

        //컷씬 so 세팅.
        CurrentActionsIndex = 0;
        if (Scenarios.ContainsKey(cutSceneName))
        {
            CurrentScenario = Scenarios[cutSceneName];
            EnableCutsceneMode();
            IsPlayCutscene.Value = true;
            SetNextCut();
        }
        else
        {
            Debug.Log("실행할 시나리오 없음.");
        }
    }

    public void EndCutscene()
    {
        IsPlayCutscene.Value = false;
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
            curAction.ActionNum = CurrentActionsIndex;
            CurrentActions.Add(curAction); //현재 액션 추가
            cutsceneHash.Add(CurrentActionsIndex);
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
            StartCoroutine(CurrentActions[i].PlayActionRoutine());
        }
    }

    public void EndAction(int num)
    {
        if (cutsceneHash.Contains(num))
        {
            _currentActionsCount--;
            if (_currentActionsCount <= 0)
                SetNextCut();
            cutsceneHash.Remove(num);
        }
        else
        {
            Debug.Log($"재생중이지 않은 액션의 종료 호출 {num}");
        }
    }

    public void SetCharacter(PlayerCutSceneController cutSceneObj, GameObject inGameObj, CharacterCutsceneData data)
    {
        if (!inGameObj && data.GetInGamePosition)
            cutSceneObj.SetPosition(inGameObj.transform.position);
        else
            cutSceneObj.SetPosition(data.position);
        cutSceneObj.SetDirection(data.isRight);
        StartCoroutine(cutSceneObj.Fader(true, 0));
        cutSceneObj.gameObject.SetActive(data.ShowCharacter);
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

    public PlayerCutSceneController GetCharacter(ESelectedCharacter character)
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
}