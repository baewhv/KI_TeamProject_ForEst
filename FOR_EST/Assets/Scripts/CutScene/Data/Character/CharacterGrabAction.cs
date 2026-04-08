using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CutScene
{
    [Serializable]
    public class CharacterGrabAction : BaseAction
    {
        public CharacterGrabAction()
        {
            _actionType = EActions.CharacterGrab;
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.Player.gameObject.SetActive(true);
            CutSceneManager.Instance.Player.SetGrab();
            CutSceneManager.Instance.EndAction(ActionNum);
            yield return null;
        }
    }
}