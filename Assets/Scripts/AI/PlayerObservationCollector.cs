using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(HandSteering))]
public class PlayerObservationCollector : MonoBehaviour, IObservationCollector
{
    private List<BoxCollider> _colliders;
    private HandSteering _handSteering;
    private HandGun _handGun;
    private void Awake()
    {
        _colliders = GetComponentsInChildren<BoxCollider>().ToList();
        _handSteering = GetComponentInChildren<HandSteering>();
        _handGun = gameObject. GetComponentInChildren<HandGun>();
    }
    

    public PlayerObservations CollectObservations(Vector3 globalPositionSphereCenterPoint)
    {
        return new PlayerObservations
        {
            AimingAt = _handGun.GetAimingAt() ?? HitType.Other,
            Hands = _handSteering.GetHandsObservation(),
            Hitboxes = CollectHitboxObservations()
        };
    }

    private List<Hitbox> CollectHitboxObservations()
    {
        var hitboxes = new List<Hitbox>();
        foreach (var collider in _colliders)
        {
            hitboxes.Add(
                new Hitbox
                {
                    Vertices = GetVertexPositionsFromCollider(collider)
                });
        }
        return hitboxes;
    }
    
    private List<Sphere3> GetVertexPositionsFromCollider(BoxCollider box)
    {
        Vector3 center = box.center;
        Vector3 size = box.size * 0.5f;

        Vector3[] localCorners = new Vector3[4]
        {
            new Vector3(-size.x, -size.y, size.z), // left lower
            new Vector3( size.x, -size.y, size.z), // right lower
            new Vector3(-size.x,  size.y, size.z), // left upper
            new Vector3( size.x,  size.y, size.z)  // right upper
        };

        List<Sphere3> sphereCorners = new List<Sphere3>();
        for (int i = 0; i < 4; i++)
        {
            var worldPos = box.transform.TransformPoint(center + localCorners[i]);
            sphereCorners.Add(new Sphere3(worldPos, _handSteering.GlobalPositionSphereCenterPoint));
        }
        
        return sphereCorners;
    }
    
}
