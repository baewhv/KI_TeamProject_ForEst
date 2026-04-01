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
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.Player.SetDirection(isRight);
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}