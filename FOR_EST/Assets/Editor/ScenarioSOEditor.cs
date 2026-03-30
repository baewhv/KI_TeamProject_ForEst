using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CutScene;

[CustomEditor(typeof(ScenarioSO))]
public class ScenarioSOEditor : Editor
{
    private SerializedProperty temp;
    private ReorderableList _actionList;
    private void OnEnable()
    {
        temp = serializedObject.FindProperty("ActionList");
        _actionList = new ReorderableList(serializedObject, temp, true, true, true, true);
        _actionList.elementHeightCallback = (index) =>
        {
            var element = temp.GetArrayElementAtIndex(index);
            if (element.isExpanded)
                return EditorGUI.GetPropertyHeight(element, true) + 10;
            else
                return EditorGUIUtility.singleLineHeight + 4;
        };
        _actionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = temp.GetArrayElementAtIndex(index);
            float lineHeight = EditorGUIUtility.singleLineHeight;

            SerializedProperty nameProp = element.FindPropertyRelative("_actionType");
            string displayName = nameProp.enumDisplayNames[nameProp.enumValueIndex];
            EActions beforeType = (EActions)nameProp.enumValueIndex;

            
            //element.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, 20, lineHeight), element.isExpanded, "");

            //Rect headerRect = new Rect(rect.x + 25, rect.y, rect.width - 25, lineHeight);
            //EditorGUI.LabelField(headerRect, displayName, EditorStyles.boldLabel);
            
            EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, rect.height - lineHeight),
                element,new GUIContent(displayName),
                true);
            if (EditorGUI.EndChangeCheck())
            {
                EActions selectedType = (EActions)nameProp.enumValueIndex;
                if(selectedType != beforeType)   
                    element.managedReferenceValue = CreateActionInstance(selectedType);
            }
        };

        _actionList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "시나리오 연출 목록 (드래그 하여 순서 변경)");
        };

        _actionList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
        {
            var menu = new GenericMenu();

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => typeof(BaseAction).IsAssignableFrom(p) && !p.IsAbstract);
            for (int i = 1; i < (int)EActions.Max; i++)
            {
                var i1 = i;
                menu.AddItem(new GUIContent(GetEnumInspectorName((EActions)i)) , false, () =>
                {
                    serializedObject.Update();//새로 추가할 때 함수.
                    int index = temp.arraySize;
                    temp.InsertArrayElementAtIndex(index);
                    temp.GetArrayElementAtIndex(index).managedReferenceValue = CreateActionInstance((EActions)i1); 
                    serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _actionList.DoLayoutList();
        
        serializedObject.ApplyModifiedProperties();
    }

    private BaseAction CreateActionInstance(EActions type)
    {
        switch (type)
        {
            case EActions.PlayDialog:
                return new PlayDialogAction();
            case EActions.FadeIn:
                return new FadeInAction();
            case EActions.FadeOut:
                return new FadeOutAction();
            case EActions.BackgroundImage:
                return new BackgroundImageAction();
            case EActions.CameraZoom:
                return new CameraZoomAction();
            case EActions.CameraMove:
                return new CameraMoveAction();
            case EActions.CameraSetTarget:
                return new CameraTargetAction();
            case EActions.CharacterMove:
                return new CharacterMoveAction();
            case EActions.CharacterDirection:
                return new CharacterDirectionAction();
            case EActions.CharacterReverse:
                return new CharacterReverseAction();
            case EActions.CharacterPlayAnimation:
                return new CharacterPlayAnimationAction();
            case EActions.Delay:
                return new DelayAction();
            default:
                return new DelayAction();
        }
    }
    
    private string GetEnumInspectorName(EActions type)
    {
        switch (type)
        {
            case EActions.PlayDialog:
                return "대사 출력";
            case EActions.FadeIn:
                return "페이드 인";
            case EActions.FadeOut:
                return "페이드 아웃";
            case EActions.BackgroundImage:
                return "배경 이미지";
            case EActions.CameraZoom:
                return "카메라 줌";
            case EActions.CameraMove:
                return "카메라 이동";
            case EActions.CameraSetTarget:
                return "카메라 대상 추적";
            case EActions.CharacterMove:
                return "캐릭터 이동";
            case EActions.CharacterDirection:
                return "캐릭터 방향";
            case EActions.CharacterReverse:
                return "캐릭터 반전";
            case EActions.CharacterPlayAnimation:
                return "캐릭터 애니메이션";
            case EActions.Delay:
                return "딜레이";
            default:
                return "에러";

        }
    }
}
