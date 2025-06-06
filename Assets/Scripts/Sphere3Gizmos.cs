using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere3Gizmos : MonoBehaviour
{
    public Sphere3 sphere = new Sphere3();
    public Vector3 zero = Vector3.zero;
    public Color color = Color.white;
    [SerializeField]
    private GameObject desiredPoint;
    
    void OnDrawGizmosSelected() { 
        sphere.FromGlobalVector3(desiredPoint.transform.position, transform.position);
       SphereGizmo();
       DrawRay();
    }

    private void Update() {
        
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
