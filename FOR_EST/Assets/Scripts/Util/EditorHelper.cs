using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class EditorHelper : MonoBehaviour
{
    private GUIStyle style;

    private void OnDrawGizmos()
    {
        Vector3 size = new Vector3(0.5f, -0.5f, 0.0f);

        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.3f);


        style = new GUIStyle(GUI.skin.box)
        {
            normal =
            {
                textColor = Color.red
            },
            alignment = TextAnchor.UpperLeft
        };
#if UNITY_EDITOR
        Handles.Label(transform.position + size, $"x : {transform.position.x}\ny : {transform.position.y}", style);
#endif
    }

    public void StartDestroy()
    {
        StartCoroutine(Destroyer());
    }

    private IEnumerator Destroyer()
    {
        yield return YieldContainer.WaitForSeconds(3.0f);
        DestroyImmediate(gameObject);
    }
}