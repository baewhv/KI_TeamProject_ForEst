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

        public override IEnumerator PlayActionRoutine()
        {
            GameObject targetObject = GameObject.Find(Target);
            if (targetObject != null)
            {
                CutSceneManager.Instance.CutsceneCamera.Follow = targetObject.transform;
            }

            yield return null;

            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}