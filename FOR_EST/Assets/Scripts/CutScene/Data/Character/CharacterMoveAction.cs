using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace CutScene
{
    [System.Serializable]
    public class CharacterMoveAction : BaseAction
    {
        public ESelectedCharacter character;
        public Vector2 position;
        public bool isForceMove;
        public CharacterMoveAction()
        {
            _actionType = EActions.CharacterMove;
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.GetCharacter(character).gameObject.SetActive(true);
            yield return CutSceneManager.Instance.GetCharacter(character).SetMoveTarget(position, isForceMove);
            CutSceneManager.Instance.EndAction(ActionNum);
        }
    }
}