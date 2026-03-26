using System;
using System.Collections.Generic;
using UnityEngine;

public class GizmoHelper : SingletonMonoBehaviour<GizmoHelper>
{
    private Dictionary<string, (Vector2 s, Vector2 e)> rays;

    protected override void Awake()
    {
        base.Awake();
        rays = new();
    }

    private void OnDrawGizmos()
    {
        foreach (var r in rays)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(r.Value.s, r.Value.e);
        }
    }

    public void SetGizmos(string key, Vector2 start, Vector2 end)
    {
        rays[key] = (start, end);
    }
}
