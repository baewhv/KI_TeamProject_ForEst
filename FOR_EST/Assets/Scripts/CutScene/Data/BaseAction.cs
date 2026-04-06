using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public abstract class BaseAction
    {
        [SerializeField] protected EActions _actionType;
        public EActions ActionType => _actionType; 
        public ENextActionType NextType = ENextActionType.Immediate;
        public int ActionNum { get; set; }

        public abstract IEnumerator PlayActionRoutine();
        private bool _isActionEnd;
    }
}   