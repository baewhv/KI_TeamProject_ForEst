using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CharacterReverseAction : BaseAction
    {
        public ESelectedCharacter character;
        public CharacterReverseAction()
        {
            _actionType = EActions.CharacterReverse;
        }
        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.GetCharacter(character).SetReverse();
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}