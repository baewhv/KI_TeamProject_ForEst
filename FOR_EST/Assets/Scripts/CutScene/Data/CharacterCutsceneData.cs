using System;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace CutScene
{
    [Serializable]
    public class CharacterCutsceneData
    {
        public ESelectedCharacter character;
        public bool ShowCharacter;        
        public bool GetInGamePosition;
        public Vector2 position;
        public bool isRight;

        public CharacterCutsceneData(ESelectedCharacter character)
        {
            this.character = character;
        }
    }
}