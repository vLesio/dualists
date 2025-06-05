using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere3Gizmos : MonoBehaviour
{
    public Sphere3 sphere = new Sphere3(Vector3.zero);
    public Vector3 zero = Vector3.zero;
    public Color color = Color.white;
    [SerializeField]
    private GameObject desiredPoint;
    
    void OnDrawGizmosSelected() { 
        sphere.FromVector3(desiredPoint.transform.localPosition);
       SphereGizmo();
       DrawRay();
    }

    private void Update() {
        
    }

    void SphereGizmo() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.TransformPoint(zero), sphere.Radius);
        
        
        Gizmos.DrawSphere(transform.TransformPoint(sphere.ToVector3()), 0.05f);
    }
    
    void DrawRay()
    {
        Gizmos.color = color;
        Gizmos.DrawRay(transform.TransformPoint(zero), transform.TransformPoint(sphere.ToVector3()-transform.TransformPoint(zero)));
    }
}
