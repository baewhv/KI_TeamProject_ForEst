using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace CutScene
{
    [System.Serializable]
    public class CharacterMoveAction : BaseAction
    {
        public CharacterMoveAction()
        {
            _actionType = EActions.CharacterMove;
        }
        public ESelectedCharacter character;
        public Vector2 position;
        public bool isForceMove;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            yield return CutSceneManager.Instance.Player.SetMoveTarget(position, isForceMove);
            CutSceneManager.Instance.EndAction();
        }
    }
}