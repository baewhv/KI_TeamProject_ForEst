using System.Collections.Generic;
using UnityEngine;
using CutScene;
using Unity.VisualScripting;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ScenarioSO", menuName = "Scriptable Objects/ScenarioSO")]
public class ScenarioSO : ScriptableObject
{
    /*
     * 시작 데이터 설정 
     * 플레이어 캐릭터
     *      - On / Off
     *      - 
     * npc - 시드
     * npc - 시드콩 -> 연출 중 몇개의 시드콩이 나타나는지?
     * 배경 이미지
     *
    */
    public CharacterCutsceneData PlayerData = new (ESelectedCharacter.Est);
    public CharacterCutsceneData SeedData = new (ESelectedCharacter.Seed);
    public CharacterCutsceneData SeedBData = new (ESelectedCharacter.Seed_B);
    public CameraCutsceneData CameraData = new();

    public List<CutsceneTriggerData> Triggers;
    public List<GameObject> CutsceneObject;
    
    
    [SerializeReference]
    public List<BaseAction> ActionList;
}
