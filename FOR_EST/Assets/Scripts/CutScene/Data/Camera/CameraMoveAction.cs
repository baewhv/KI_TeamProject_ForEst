using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace CutScene
{
    [System.Serializable]
    public class CameraMoveAction : BaseAction
    {
        public CameraMoveAction()
        {
            _actionType = EActions.CameraMove;
        }
        public Vector2 position;
        public float time;
        public override void InitAction()
        {
            Debug.Log($"{GetType()} : 시작");
        }

        public override void Update()
        {

        }

        public override IEnumerator PlayActionRoutine()
        {
            CinemachineCamera cam = CutSceneManager.Instance.CutsceneCamera;
            cam.Follow = null;

            Vector3 startPos = cam.transform.position;
            Vector3 endPos = new Vector3(position.x, position.y, startPos.z);

            float currentTime = 0f;

            if (time > 0f)
            {
                while (currentTime < time)
                {
                    currentTime = currentTime + Time.deltaTime;
                    float percent = currentTime / time;

                    cam.transform.position = Vector3.Lerp(startPos, endPos, percent);
                    yield return null;
                }
            }

            cam.transform.position = endPos;
            Debug.Log($"{GetType()} : 종료");
            CutSceneManager.Instance.EndAction();
        }
    }
}