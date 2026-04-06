using System.Collections;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class BackgroundImageAction : BaseAction
    {
        public bool hideBackground;
        public Sprite backgroundImage;
        public BackgroundImageAction()
        {
            _actionType = EActions.BackgroundImage;
        }

        public override IEnumerator PlayActionRoutine()
        {
            if(hideBackground || !backgroundImage)
            {
                CutSceneManager.Instance.CinemaUI.BackgroundImage.color = new Color(1, 1, 1, 0);
                CutSceneManager.Instance.CinemaUI.BackgroundImage.sprite = null;
            }
            else
            {
                CutSceneManager.Instance.CinemaUI.BackgroundImage.color = new Color(1, 1, 1, 1);
                CutSceneManager.Instance.CinemaUI.BackgroundImage.sprite = backgroundImage;
            }

            CutSceneManager.Instance.EndAction(ActionNum);
            yield return null;
        }
    }
}