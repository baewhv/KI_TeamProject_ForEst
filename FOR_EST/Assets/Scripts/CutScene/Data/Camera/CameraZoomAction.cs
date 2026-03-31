using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CameraZoomAction : BaseAction
    {
        public CameraZoomAction()
        {
            _actionType = EActions.CameraZoom;
        }
        public float value = 1.0f;
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