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
                CutSceneManager.Instance.CinemaUI.OffBackground();
            }
            else
            {
                CutSceneManager.Instance.CinemaUI.OnBackground();
                CutSceneManager.Instance.CinemaUI.BackgroundImage.sprite = backgroundImage;
            }

            CutSceneManager.Instance.EndAction(ActionNum);
            yield return null;
        }
    }
}