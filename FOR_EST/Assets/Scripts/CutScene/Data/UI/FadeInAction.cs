using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CutScene
{
    [System.Serializable]
    public class FadeInAction : BaseAction
    {
        private Image _fader;
        public float time;
        private float _currentTime;

        public FadeInAction()
        {
            _actionType = EActions.FadeIn;
        }


        public override void InitAction()
        {
            _fader = CutSceneManager.Instance.CinemaUI.Fader;
            _currentTime = 0.0f;
            _fader.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        }

        public override void Update()
        {
        }

        public override IEnumerator PlayActionRoutine()
        {
            while (_currentTime < time)
            {
                _currentTime += Time.deltaTime;
                _fader.color = new Color(0, 0, 0, _fader.color.a - (1.0f / time * Time.deltaTime));
                yield return null;
            }
            _fader.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            CutSceneManager.Instance.EndAction();
            //yield return null; // while문이 안돌 경우 예비용 return
        }
    }
}