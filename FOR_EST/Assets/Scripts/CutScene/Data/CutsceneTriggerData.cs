using System;
using UnityEngine;

namespace CutScene
{
    [Serializable]
    public class CutsceneTriggerData
    {
        public Vector2 position; //위치
        public string triggerSOName;
        public string triggerTargetTag;
    }
}