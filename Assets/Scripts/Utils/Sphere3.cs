using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Sphere3 {

    public float Radius = 0f;
    public float Theta = 0f; // Left/Right - [-π, π]
    public float Phi = 0f;   // Up/Down - [-π/2, π/2]
    public Transform globalReferenceTransform;
    
    /// <summary>
    /// Constructs a Sphere3 object from a position in global coordinates. It transforms the given coordinates into spherical coordinates relative to a global reference point.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="globalReferencePoint"></param>
    public Sphere3(Vector3 position, Transform globalReferencePoint) {
        FromGlobalVector3(position, globalReferencePoint);
    }
    public Sphere3(float radius, float theta, float phi, Transform globalReferencePoint) {
        Radius = radius;
        Theta = theta;
        Phi = phi;
        globalReferenceTransform = globalReferencePoint;
    }
    public Vector3 ToLocalVector3() {
        var x = Radius * Mathf.Sin(Theta) * Mathf.Cos(Phi);
        var y = Radius * Mathf.Sin(Theta) * Mathf.Sin(Phi);
        var z = Radius * Mathf.Cos(Theta);
        return new Vector3(x, y, z);
    }
    public Vector3 ToGlobalVector3() {
        /*var x = Radius * Mathf.Sin(Theta) * Mathf.Cos(Phi);
        var y = Radius * Mathf.Sin(Theta) * Mathf.Sin(Phi);
        var z = Radius * Mathf.Cos(Theta);
        return new Vector3(x, y, z) + globalReferenceTransform;*/
        return globalReferenceTransform.TransformPoint(ToLocalVector3());
    }
    public void FromVector3(Vector3 position, Transform globalReferencePoint)
    {
        Radius = position.magnitude;
        if (Radius == 0) {
            Theta = 0;
        }
        else {
            Theta = Mathf.Acos(position.z / Radius);
        }
        Phi = Mathf.Atan2(position.y, position.x);
        globalReferenceTransform = globalReferencePoint;
    }
    public void FromGlobalVector3(Vector3 position, Transform globalReferencePoint)
    {
        var localPos = globalReferencePoint.InverseTransformPoint(position);
        FromVector3(localPos, globalReferencePoint);
        globalReferenceTransform = globalReferencePoint;
    }
    
    
    public Vector3 AsSphericalVector()
    {
        return new Vector3(Radius, Theta, Phi);
    }

    /// <summary>
    ///     Transforms a normalized sphere observation into a Sphere3 object.
    /// </summary>
    /// <param name="normR"> Normalized radius in the range [-1, 1]</param>
    /// <param name="normTheta"> Normalized theta in the range [-1, 1]</param>
    /// <param name="normPhi"> Normalized phi in the range [-1, 1]</param>
    /// <param name="globalReferencePoint"> Global center point of the sphere</param>
    /// <param name="maxR"> Maximum radius of the sphere.</param>
    /// <param name="minR"> Minimum radius of the sphere, default is 0</param>
    /// <returns> Sphere3 object representing a point in spherical coordinates.</returns>
    public static Sphere3 FromNormalized(float normR, float normTheta, float normPhi, Transform globalReferencePoint, float maxR, float minR = 0f)
    {
        float r = (normR + 1f / 2f); // [-1, 1] -> [0, 1]
        //Debug.LogWarning(r);
        float radius = Mathf.Lerp(minR, maxR, r); // [0, 1] -> [min, max]
        float theta = normTheta * Mathf.PI; // [-π, π]
        float phi = normPhi * (Mathf.PI / 2f); // [-π/2, π/2]

        return new Sphere3(radius, theta, phi, globalReferencePoint);
    }

    public override string ToString()
    {
        return $"Sphere3: Radius={Radius}, Theta={Theta}, Phi={Phi}, GlobalReferencePoint={globalReferenceTransform}";
    }
}

