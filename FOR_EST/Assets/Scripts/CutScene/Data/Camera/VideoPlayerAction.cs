using System.Collections;
using UnityEngine;
using UnityEngine.Video; 

namespace CutScene
{
    [System.Serializable]
    public class VideoPlayerAction : BaseAction
    {
        public VideoPlayerAction()
        {
            _actionType = EActions.VideoPlay;
        }

        public VideoClip clip; 

        public override IEnumerator PlayActionRoutine()
        {
            UnityEngine.Video.VideoPlayer videoPlayer = CutSceneManager.Instance.VideoPlayer;

            if (videoPlayer != null && clip != null)
            {
                videoPlayer.clip = clip;

                videoPlayer.Prepare();

                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }

                videoPlayer.Play();

                while (videoPlayer.isPlaying)
                {
                    yield return null;
                }

                videoPlayer.Stop();
            }

            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction(ActionNum);
        }
    }
}