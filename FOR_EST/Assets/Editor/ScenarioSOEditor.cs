using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CutScene;

[CustomEditor(typeof(ScenarioSO))]
public class ScenarioSOEditor : Editor
{
    private SerializedProperty _actionsProperty;
    private SerializedProperty _estProperty;
    private SerializedProperty _seedProperty;
    private SerializedProperty _seedBProperty;
    private SerializedProperty _cameraProperty;
    private SerializedProperty _imgListProperty;
    
    private ReorderableList _actionList;
    private List<Vector2> points = new();
    private static GameObject go;

    private void OnEnable()
    {
        _actionsProperty = serializedObject.FindProperty("ActionList");
        _estProperty = serializedObject.FindProperty("PlayerData");
        _seedProperty = serializedObject.FindProperty("SeedData");
        _seedBProperty = serializedObject.FindProperty("SeedBData");
        _cameraProperty = serializedObject.FindProperty("CameraData");
        
        _imgListProperty = serializedObject.FindProperty("ImageResource");
        
        _actionList = new ReorderableList(serializedObject, _actionsProperty, true, true, true, true);
        _actionList.elementHeightCallback = (index) =>
        {
            var element = _actionsProperty.GetArrayElementAtIndex(index);
            if (element.isExpanded)
                return EditorGUI.GetPropertyHeight(element, true) + 10;
            else
                return EditorGUIUtility.singleLineHeight + 4;
        };
        _actionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = _actionsProperty.GetArrayElementAtIndex(index);
            float lineHeight = EditorGUIUtility.singleLineHeight;

            SerializedProperty nameProp = element.FindPropertyRelative("_actionType");
            string displayName = nameProp.enumDisplayNames[nameProp.enumValueIndex];
            EActions beforeType = (EActions)nameProp.enumValueIndex;

            EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height - lineHeight),
                element, new GUIContent(displayName), true);
            if (EditorGUI.EndChangeCheck())
            {
                EActions selectedType = (EActions)nameProp.enumValueIndex;
                if (selectedType != beforeType)
                    element.managedReferenceValue = CreateActionInstance(selectedType);
            }
        };

        _actionList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "시나리오 연출 목록 (드래그 하여 순서 변경)"); };

        _actionList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
        {
            var menu = new GenericMenu();

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => typeof(BaseAction).IsAssignableFrom(p) && !p.IsAbstract);
            for (int i = 1; i < (int)EActions.Max; i++)
            {
                var i1 = i;
                menu.AddItem(new GUIContent(GetEnumInspectorName((EActions)i)), false, () =>
                {
                    serializedObject.Update(); //새로 추가할 때 함수.
                    int index = _actionsProperty.arraySize;
                    _actionsProperty.InsertArrayElementAtIndex(index);
                    _actionsProperty.GetArrayElementAtIndex(index).managedReferenceValue = CreateActionInstance((EActions)i1);
                    serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        };
    }

    public void OnDisable()
    {
        if(go)
            go.GetComponent<EditorHelper>().StartDestroy();
        go = null;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("연출 초기 설정");
        EditorGUILayout.PropertyField(_estProperty, new GUIContent("에스트 설정"), true);
        EditorGUILayout.PropertyField(_seedProperty, new GUIContent("시드 설정"), true);
        EditorGUILayout.PropertyField(_seedBProperty, new GUIContent("시드콩 설정"), true);
        EditorGUILayout.PropertyField(_cameraProperty, new GUIContent("카메라 설정"), true);
        
        EditorGUILayout.PropertyField(_imgListProperty, new GUIContent("이미지 설정(준비중)"), true);
            
        _actionList.DoLayoutList();
        if (GUILayout.Button("위치 확인용(테스트중)"))
        {
            Vector2 pos = new Vector2(5, 4);
            CreateDummy(pos);
        }
        serializedObject.ApplyModifiedProperties();
    }

    public void CreateDummy(Vector2 pos)
    {
        if(go) DestroyImmediate(go);
        go = new GameObject();
        go.transform.position = pos;
        go.name = "[TEMP] Cutscene Helper";
        go.hideFlags = HideFlags.DontSave | HideFlags.DontSaveInEditor;
        go.AddComponent<EditorHelper>();
        

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
            case EActions.CharacterFader:
                return new CharacterFaderAction();
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
            case EActions.CharacterFader:
                return "캐릭터 페이드";
            case EActions.Delay:
                return "딜레이";
            default:
                return "에러";
        }
    }
}