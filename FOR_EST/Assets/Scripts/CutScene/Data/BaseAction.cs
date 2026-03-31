using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public abstract class BaseAction
    {
        [SerializeField] protected EActions _actionType;
        public EActions ActionType => _actionType; 
        public ENextActionType NextType;

        public abstract void InitAction();
        public abstract void Update();
        public abstract IEnumerator PlayActionRoutine();
    }
}   