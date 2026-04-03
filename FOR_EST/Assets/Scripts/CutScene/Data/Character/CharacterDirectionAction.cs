using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace CutScene
{
    [System.Serializable]
    public class CharacterDirectionAction : BaseAction
    {
        public ESelectedCharacter character;
        public bool isRight;

        public CharacterDirectionAction()
        {
            _actionType = EActions.CharacterDirection;
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.GetCharacter(character).SetDirection(isRight);
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}