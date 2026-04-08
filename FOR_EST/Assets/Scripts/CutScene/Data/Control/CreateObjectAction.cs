using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CreateObjectAction : BaseAction
    {
        public int objectNumber;
        public Vector2 position;
        public CreateObjectAction()
        {
            _actionType = EActions.CreateObject;
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.CreateObject(objectNumber, position);
            yield return null;
            CutSceneManager.Instance.EndAction(ActionNum);
        }
    }
}