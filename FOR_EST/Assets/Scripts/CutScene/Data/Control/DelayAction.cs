using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class DelayAction : BaseAction
    {
        public DelayAction()
        {
            _actionType = EActions.Delay;
        }
        
        public float delayTime;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }

        public override void Update()
        {
        }

        public override IEnumerator PlayActionRoutine()
        {
            yield return YieldContainer.WaitForSeconds(delayTime);
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}