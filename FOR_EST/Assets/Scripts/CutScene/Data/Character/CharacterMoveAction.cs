using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CharacterMoveAction : BaseAction
    {
        public CharacterMoveAction()
        {
            _actionType = EActions.CharacterMove;
        }
        public string character;
        public Vector2 position;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            yield return CutSceneManager.Instance.Player.SetMoveTarget(position);
            CutSceneManager.Instance.EndAction();
        }
    }
}