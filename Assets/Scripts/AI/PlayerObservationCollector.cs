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
        _colliders = GetComponentInChildren<PlayerHitbox>().gameObject.GetComponents<BoxCollider>().ToList();
        if (_colliders.Count == 0)
        {
            Debug.LogError("AIPlayerController: There are no colliders in the PlayerHitbox component.");    
        }
        _handSteering = GetComponentInChildren<HandSteering>();
        if (_handSteering == null)
        {
            Debug.LogError("AIPlayerController: There is no HandSteering component in the object's hierarchy.");
        }
        _handGun = gameObject. GetComponentInChildren<HandGun>();
        if (_handGun == null)
        {
            Debug.LogError("AIPlayerController: There is no HandGun component in the object's hierarchy.");
        }
    }
    

    public PlayerObservations CollectObservations(Transform globalSphereCenterPoint)
    {
        return new PlayerObservations
        {
            AimingAt = _handGun.GetAimingAt() ?? HitType.Other,
            HandObservations = _handSteering.GetHandsObservation(globalSphereCenterPoint),
            Hitboxes = CollectHitboxObservations(globalSphereCenterPoint),
            IsGunUsable = _handGun.IsUsable,
            GunAmmoPercentage = _handGun.AmmoLeft
            
        };
    }

    private List<Hitbox> CollectHitboxObservations(Transform globalSphereCenterPoint)
    {
        var hitboxes = new List<Hitbox>();
        foreach (var collider in _colliders)
        {
            hitboxes.Add(
                new Hitbox
                {
                    Vertices = GetVertexPositionsFromCollider(collider, globalSphereCenterPoint)
                });
        }
        return hitboxes;
    }
    
    private List<Sphere3> GetVertexPositionsFromCollider(BoxCollider box, Transform globalSphereCenterPoint)
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
            sphereCorners.Add(new Sphere3(worldPos, globalSphereCenterPoint));
        }
        
        return sphereCorners;
    }
    
    public float CalculateAngleBetweenPlayerWeaponAndEnemy(HandGun playerHandGun, Transform playerTransform)
    {
        Vector3 weaponDirection = playerHandGun.GetDirection();
        weaponDirection.y = 0;
        weaponDirection.Normalize();

        Vector3 targetDirection = transform.position - playerTransform.position;
        targetDirection.y = 0;
        targetDirection.Normalize();

        //Debug.LogError($"Weapon direction: {weaponDirection}, Target direction: {targetDirection}");
        //float angle = Vector3.SignedAngle(weaponDirection, targetDirection, Vector3.up);
        //Debug.LogWarning(angle);
        //float angleDot = Vector3.Dot(weaponDirection, targetDirection);
        //Debug.LogWarning(angleDot);
        float angle = Vector3.Angle(weaponDirection, targetDirection); // 0–180

      
        float bias;
        if (angle <= 90f)
        {
            bias = Mathf.Lerp(1f, 0f, angle / 90f); // 0 -> 1, 90° -> 0
        }
        else
        {
            bias = Mathf.Lerp(0f, -1f, (angle - 90f) / 90f); // 90° -> 0, 180° -> -1
        }

        return bias;
    }
}
