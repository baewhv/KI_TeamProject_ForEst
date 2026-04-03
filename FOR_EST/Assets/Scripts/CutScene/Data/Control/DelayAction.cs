using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class DelayAction : BaseAction
    {
        public float delayTime;
        public DelayAction()
        {
            _actionType = EActions.Delay;
        }

        public override IEnumerator PlayActionRoutine()
        {
            yield return YieldContainer.WaitForSeconds(delayTime);
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}