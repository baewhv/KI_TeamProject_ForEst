using CutScene;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(CharacterCutsceneData))]
public class CharacterCutsceneDataProperty : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        //Rect contentRect = EditorGUI.PrefixLabel(position, label);
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        if (property.isExpanded)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ShowCharacter"),
                new GUIContent(" 등장여부", "연출 시작부터 보여줄지 결정합니다."));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("GetInGamePosition"),
                new GUIContent(" 연출 전 위치 가져오기", "인게임의 위치를 그대로 가져올지 결정합니다."));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("position"),
                new GUIContent(" 위치", "해당 좌표에 배치합니다."));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("isRight"),
                new GUIContent(" 오른쪽 보기", "보는 방향을 설정합니다. 체크 시 오른쪽을 봅니다."));
            
        }

        EditorGUI.EndProperty();

    }
}
