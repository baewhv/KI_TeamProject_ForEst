using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace CutScene
{
    [System.Serializable]
    public class CameraZoomAction : BaseAction
    {
        public float value = 1.0f;
        public float time;
        public CameraZoomAction()
        {
            _actionType = EActions.CameraZoom;
        }

        public override IEnumerator PlayActionRoutine()
        {
            CinemachineCamera cam = CutSceneManager.Instance.CutsceneCamera;

            float startSize = cam.Lens.OrthographicSize;
            float endSize = value;
            float currentTime = 0f;

            if (time > 0f)
            {
                while (currentTime < time)
                {
                    currentTime = currentTime + Time.deltaTime;
                    float percent = currentTime / time;

                    LensSettings lens = cam.Lens;
                    lens.OrthographicSize = Mathf.Lerp(startSize, endSize, percent);
                    cam.Lens = lens;

                    yield return null;
                }
            }

            LensSettings finalLens = cam.Lens;
            finalLens.OrthographicSize = endSize;
            cam.Lens = finalLens;

            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}