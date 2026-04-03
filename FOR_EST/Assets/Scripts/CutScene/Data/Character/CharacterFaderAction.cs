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
        
        public override void InitAction()
        {
            
        }

        public override void Update()
        {
            
        }

        public override IEnumerator PlayActionRoutine()
        {
            Debug.Log("캐릭터 페이드 시작");

            yield return CutSceneManager.Instance.GetCharacter(character).Fader(isFadeIn, time);
            Debug.Log("캐릭터 페이드 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}