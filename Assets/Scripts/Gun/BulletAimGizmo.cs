using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAimGizmo : MonoBehaviour
{
    public bool show = true;
    public float gizmoLength = 2.0f;
    public Color gizmoColor = Color.red;

    void OnDrawGizmos()
    {
        if (!show) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(transform.position, -transform.up * gizmoLength);
    }
}
