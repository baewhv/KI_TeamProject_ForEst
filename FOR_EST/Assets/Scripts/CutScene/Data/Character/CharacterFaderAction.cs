using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CutScene
{
    [Serializable]
    public class CharacterFaderAction : BaseAction
    {
        private float _currentTime;
        private Image _fader;
        public ESelectedCharacter character;
        public bool isFadeIn;
        public float time;

        public CharacterFaderAction()
        {
            _actionType = EActions.CharacterFader;
        }

        public override IEnumerator PlayActionRoutine()
        {
            CutSceneManager.Instance.GetCharacter(character).gameObject.SetActive(true);
            yield return CutSceneManager.Instance.GetCharacter(character).Fader(isFadeIn, time);
            CutSceneManager.Instance.EndAction(ActionNum);
        }
    }
}