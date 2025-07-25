﻿
using System;
using UnityEngine;
using UnityEngine.Events;

public class HandGun : MonoBehaviour, IResettable {
    [Header("Pistol settings")]
    [SerializeField] private int startAmmo = 20;
    [SerializeField] private float cooldownTime = 0.5f;
    
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletStartPosition;
    [SerializeField] private Transform directionMaker;

    [Header("Events")]
    [SerializeField] private UnityEvent outOfAmmo;

    private int _currentAmmo;
    private float _lastShootTime;

    private IPlayerController _owner;

    private void Awake()
    {
        _owner = GetComponentInParent<IPlayerController>();
        if (_owner == null)
            Debug.LogWarning("[HandGun] No IPlayerController found in parent! The gun will not function with controller actions.");
        
        _currentAmmo = startAmmo;
        _lastShootTime = -999f;
    }
    
    public void Reset() {
        _currentAmmo = startAmmo;
        _lastShootTime = -999f;
    }

    public void Shoot() {
        if (IsOnCooldown() || IsOutOfAmmo()) return;
        
        SpawnNewBullet();
        _currentAmmo--;
        _lastShootTime = Time.time;
        if (_currentAmmo <= 0) { // Notifying IPlayerController that the gun is out of ammo
            if (_owner != null) {
                _owner.OnOutOfAmmo();
            }
        }
    }

    public bool IsOnCooldown() {
        return Time.time - _lastShootTime <= cooldownTime;
    }

    public bool IsOutOfAmmo() {
        return _currentAmmo <= 0;
    }

    public float AmmoLeft => (float)_currentAmmo /(float)startAmmo;
    public bool IsUsable => !(IsOnCooldown() || IsOutOfAmmo()); 
    
    private void SpawnNewBullet()
    {
        var bullet = Instantiate(bulletPrefab, bulletStartPosition.position, bulletStartPosition.rotation,
            BulletManager.I.transform);
        bullet.GetComponent<Bullet>()?.StartBullet(bulletStartPosition.position - directionMaker.position);
    }

    public HitType? GetAimingAt()
    {
        
        if(Physics.Raycast(bulletStartPosition.position, bulletStartPosition.position - directionMaker.position, out var hit))
        {
            var col = hit.collider;
            if (col.CompareTag("Dualist"))
            {
                return HitType.Player;
            }
            else if (col.CompareTag("Shield"))
            {
                return HitType.Shield;
            }
            else 
            {
                return HitType.Other;
            }
        }

        return null;
    }

    public Vector3 GetDirection()
    {
        return (bulletStartPosition.position - directionMaker.position).normalized;
    }
}
