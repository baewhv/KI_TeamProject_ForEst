using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CharacterPlayAnimationAction : BaseAction
    {
        public CharacterPlayAnimationAction()
        {
            _actionType = EActions.CharacterPlayAnimation;
        }
        public string character;
        public string animation;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.Player.SetAnimation(animation);
            while (CutSceneManager.Instance.Player.DoAction)
            {
                
            }
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}