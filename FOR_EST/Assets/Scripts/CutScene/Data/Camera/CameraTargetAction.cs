using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CameraTargetAction : BaseAction
    {
        public CameraTargetAction()
        {
            _actionType = EActions.CameraSetTarget;
        }
        public string Target;
        public override void PlayAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            yield return YieldContainer.WaitForSeconds(10);
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}