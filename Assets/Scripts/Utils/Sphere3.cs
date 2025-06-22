using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public class Sphere3 {

    public float Radius = 0f;
    public float Theta = 0f; // Left/Right - [-π, π]
    public float Phi = 0f;   // Up/Down - [-π/2, π/2]
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
    /// <param name="center"> Global center point of the sphere</param>
    /// <param name="maxR"> Maximum radius of the sphere.</param>
    /// <param name="minR"> Minimum radius of the sphere, default is 0</param>
    /// <returns> Sphere3 object representing a point in spherical coordinates.</returns>
    public static Sphere3 FromNormalized(float normR, float normTheta, float normPhi, Vector3 center, float maxR, float minR = 0f)
    {
        float r = (normR + 1f / 2f); // [-1, 1] -> [0, 1]
        float radius = Mathf.Lerp(minR, maxR, r); // [0, 1] -> [min, max]
        float theta = normTheta * Mathf.PI; // [-π, π]
        float phi = normPhi * (Mathf.PI / 2f); // [-π/2, π/2]

        return new Sphere3(radius, theta, phi, center);
    }

    public override string ToString()
    {
        return $"Sphere3: Radius={Radius}, Theta={Theta}, Phi={Phi}, GlobalPositionReferencePoint={globalPositionReferencePoint}";
    }
}

