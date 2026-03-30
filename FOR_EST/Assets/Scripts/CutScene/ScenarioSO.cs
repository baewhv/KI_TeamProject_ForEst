using System.Collections.Generic;
using UnityEngine;
using CutScene;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ScenarioSO", menuName = "Scriptable Objects/ScenarioSO")]
public class ScenarioSO : ScriptableObject
{
    [SerializeReference]
    public List<BaseAction> ActionList;
}
