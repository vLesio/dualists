using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere3Gizmos : MonoBehaviour
{
    public Sphere3 sphere;
    public Vector3 zero = Vector3.zero;
    public Color color = Color.white;
    public GameObject desiredPoint;
    //public Transform globalSphereCenterPoint;
    
    void OnDrawGizmos() { 
        sphere = new Sphere3(desiredPoint.transform.position, transform/*globalSphereCenterPoint*/);
        SphereGizmo();
        DrawRay();
    }
    
    void SphereGizmo() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.TransformPoint(zero), sphere.Radius);
        Gizmos.DrawSphere(sphere.ToGlobalVector3(), 0.05f);
    }
    
    void DrawRay()
    {
        Gizmos.color = color;
        Gizmos.DrawRay(transform.TransformPoint(zero), transform.TransformDirection(sphere.ToLocalVector3()));
    }
}
