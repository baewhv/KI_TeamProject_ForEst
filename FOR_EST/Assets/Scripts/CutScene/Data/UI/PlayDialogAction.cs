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
        public bool isEmptyPositionDialog;
        public Vector2 dialogPosition;

        public override IEnumerator PlayActionRoutine()
        {
            if (isEmptyPositionDialog)
            {
                CutSceneManager.Instance.EmptyObject.transform.position = dialogPosition;
            }
            Dialogue.Instance.StartDialog(dialogNumber);
            while (Dialogue.Instance.IsPlay)
            {
                yield return null;
            }
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}