using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CameraMoveAction : BaseAction
    {
        public CameraMoveAction()
        {
            _actionType = EActions.CameraMove;
        }
        public Vector2 position;
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