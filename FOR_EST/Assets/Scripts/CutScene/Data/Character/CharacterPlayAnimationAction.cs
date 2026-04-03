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
        public ESelectedCharacter character;
        public string animation;
        public bool TrueAndFalse;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.GetCharacter(character).SetAnimation(animation, TrueAndFalse);
            while (CutSceneManager.Instance.GetCharacter(character).DoAction)
            {
                
            }
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}