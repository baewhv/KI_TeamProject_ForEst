using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class PlayDialogAction : BaseAction
    {
        public PlayDialogAction()
        {
            _actionType = EActions.PlayDialog;
        }
        public int dialogNumber;
        public string dialogTarget;
        public Vector2 dialogPosition;

        public override IEnumerator PlayActionRoutine()
        {
            DialogueTest.Instance.StartDialog(dialogNumber);
            while (DialogueTest.Instance.IsPlay)
            {
                yield return null;
            }
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}