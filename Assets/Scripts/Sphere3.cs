using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public class Sphere3 {

    public float Radius = 0f;
    public float Theta = 0f; // Left/Right
    public float Phi = 0f;   // Up/Down
    public Vector3 globalPositionReferencePoint = Vector3.zero;
    public Sphere3() {
        Radius = 0;
        Theta = 0;
        Phi = 0;
        globalPositionReferencePoint = Vector3.zero;
    }
    public Sphere3(Vector3 position, Vector3 globalReferencePoint) {
        Radius = position.magnitude;
        Theta = Mathf.Atan2(position.z, position.x);
        Phi = Mathf.Acos(position.y / Radius);
        globalPositionReferencePoint = globalReferencePoint;
    }
    public Sphere3(float radius, float theta, float phi, Vector3 globalReferencePoint) {
        Radius = radius;
        Theta = theta;
        Phi = phi;
        globalPositionReferencePoint = globalReferencePoint;
    }
    public Vector3 ToLocalVector3() {
        var x = Radius * Mathf.Sin(Theta) * Mathf.Cos(Phi);
        var y = Radius * Mathf.Sin(Theta) * Mathf.Sin(Phi);
        var z = Radius * Mathf.Cos(Theta);
        return new Vector3(x, y, z);
    }
    public Vector3 ToGlobalVector3() {
        var x = Radius * Mathf.Sin(Theta) * Mathf.Cos(Phi);
        var y = Radius * Mathf.Sin(Theta) * Mathf.Sin(Phi);
        var z = Radius * Mathf.Cos(Theta);
        return new Vector3(x, y, z) + globalPositionReferencePoint;
    }
    public void FromVector3(Vector3 position, Vector3 globalReferencePoint)
    {
        Radius = position.magnitude;
        if (Radius == 0) {
            Theta = 0;
        }
        else {
            Theta = Mathf.Acos(position.z / Radius);
        }
        Phi = Mathf.Atan2(position.y, position.x);
        globalPositionReferencePoint = globalReferencePoint;
    }
    public void FromGlobalVector3(Vector3 position, Vector3 globalReferencePoint)
    {
        var localPos = position - globalReferencePoint;
        FromVector3(localPos, globalReferencePoint);
        globalPositionReferencePoint = globalReferencePoint;
    }
}

public static class CoordinatesTranslator
{
    
}
