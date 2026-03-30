using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class FadeOutAction : BaseAction
    {
        public FadeOutAction()
        {
            _actionType = EActions.FadeOut;
        }
        public float time;
        public override void PlayAction()
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