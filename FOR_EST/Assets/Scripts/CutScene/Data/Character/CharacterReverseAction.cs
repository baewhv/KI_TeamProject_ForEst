using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CharacterReverseAction : BaseAction
    {
        public CharacterReverseAction()
        {
            _actionType = EActions.CharacterReverse;
        }
        public string character;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }
        
        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.Player.SetReverse();
            CutSceneManager.Instance.EndAction();
            yield return null;
        }
    }
}