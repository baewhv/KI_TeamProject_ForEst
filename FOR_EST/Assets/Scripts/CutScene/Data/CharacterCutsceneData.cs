using System;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

namespace CutScene
{
    [Serializable]
    public class CharacterCutsceneData
    {
        public bool ShowCharacter;        
        public bool GetInGamePosition;
        public Vector2 position;
        public bool isRight;
    }
}