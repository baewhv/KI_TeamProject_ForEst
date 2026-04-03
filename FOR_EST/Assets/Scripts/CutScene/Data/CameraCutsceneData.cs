using System;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

namespace CutScene
{
    [Serializable]
    public class CameraCutsceneData
    {
        public bool followTarget;
        public ESelectedCharacter target;
        public Vector2 position;
        [Min(0.1f)]public float zoom; 
    }
}