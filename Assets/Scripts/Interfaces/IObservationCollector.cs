using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public enum HitType
{
    Player,
    Shield,
    Other
}

public struct Hitbox
{
    public List<Sphere3> Vertices;
}

public struct PlayerObservations
{
    public List<Hitbox> Hitboxes;
    public HitType AimingAt;
    public bool IsGunUsable;
    public float GunAmmoPercentage;
    
    public HandsObservation HandObservations;
}

public interface IObservationCollector
{
    PlayerObservations CollectObservations(Transform globalSphereCenterPoint);
    float CalculateAngleBetweenPlayerWeaponAndEnemy(HandGun playerHandGun, Transform playerTransform);
}
