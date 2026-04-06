using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CameraTargetAction : BaseAction
    {
        public ESelectedCharacter character;
        public CameraTargetAction()
        {
            _actionType = EActions.CameraSetTarget;
        }

        public override IEnumerator PlayActionRoutine()
        {
            GameObject targetObject = CutSceneManager.Instance.GetCharacter(character).gameObject;
            if (targetObject != null)
            {
                CutSceneManager.Instance.CutsceneCamera.Follow = targetObject.transform;
            }
            yield return null;
            CutSceneManager.Instance.EndAction(ActionNum);
        }
    }
}