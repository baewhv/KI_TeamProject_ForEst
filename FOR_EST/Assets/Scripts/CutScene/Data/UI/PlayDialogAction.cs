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
        public string dialogNumber;
        public string dialogTarget;
        public Vector2 dialogPosition;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            yield return YieldContainer.WaitForSeconds(1);
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}