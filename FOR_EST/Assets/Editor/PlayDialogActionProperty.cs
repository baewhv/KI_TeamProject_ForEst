using CutScene;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(PlayDialogAction))]
public class PlayDialogActionProperty : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight + 2f;

        if (!property.isExpanded)
            return lineHeight;
        int lines = 3;

        if (property.FindPropertyRelative("isEmptyPositionDialog").boolValue)
            lines += 1;
        return lines * lineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect fieldRect = new Rect(position.x, position.y, position.width, lineHeight);

        property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, label);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            fieldRect.y += lineHeight + 2f;
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("dialogNumber"),
                new GUIContent("시작 대사 번호", "출력할 대사의 테이블 번호입니다. NextID를 통해 다음 대사를 출력합니다. NextID가 0이면 해당 대사 이후 종료합니다."));
            
            var isEmptyPosDialog = property.FindPropertyRelative("isEmptyPositionDialog");
            fieldRect.y += lineHeight + 2f;
            EditorGUI.PropertyField(fieldRect, isEmptyPosDialog, new GUIContent("허공 대사 출력 여부",
                    "허공에 출력할 대사라면 위치를 지정할 수 있습니다. 다른곳에 출력해야 한다면 NextID를 0으로 둬 대사를 끊어주셔야 합니다."));
            if (isEmptyPosDialog.boolValue)
            {
                fieldRect.y += lineHeight + 2f;
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("dialogPosition"),
                    new GUIContent("허공 대사 위치", "허공에 출력할 대사의 위치입니다."));
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}