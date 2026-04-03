using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CharacterPlayAnimationAction : BaseAction
    {
        public ESelectedCharacter character;
        public string animation;
        public bool TrueAndFalse;
        public CharacterPlayAnimationAction()
        {
            _actionType = EActions.CharacterPlayAnimation;
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