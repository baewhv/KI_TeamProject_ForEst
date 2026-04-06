using CutScene;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(CameraCutsceneData))]
public class CameraCutsceneDataProperty : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            var target = property.FindPropertyRelative("followTarget");
            EditorGUILayout.PropertyField(target,
                new GUIContent("타겟", "체크 시 지정된 캐릭터로 타겟팅합니다. 체크하지 않으면 좌표로 이동합니다."));
            if (target.boolValue)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("target"),
                    new GUIContent("타겟 캐릭터", "타겟팅할 캐릭터 목록입니다."));
            }
            else
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("position"),
                    new GUIContent("위치", "해당 좌표에 배치합니다."));
            }
            EditorGUILayout.PropertyField(property.FindPropertyRelative("zoom"),
                new GUIContent("줌", "줌 값을 설정합니다. 기본값 11.5, 작으면 줌인, 크면 줌아웃"));
            
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();

    }
}
