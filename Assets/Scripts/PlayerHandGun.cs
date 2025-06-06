using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerHandGun : MonoBehaviour
{
    [Header("Setup")]
    public string playerName = "PlayerVR"; 
    public InputAction InputActions;
    public GameObject bulletPrefab;
    public Transform bulletStartPosition;
    public Transform directionMaker;

    [Header("Ammo")]
    public int maxAmmo = 5;
    private int _currentAmmo;

    private PointableUnityEventWrapper _eventWrapper;

    private void Awake()
    {
        _eventWrapper = GetComponent<PointableUnityEventWrapper>();
        _eventWrapper.WhenSelect.AddListener(OnGrab);
        _eventWrapper.WhenUnselect.AddListener(OnRelease);
        InputActions.performed += OnUse;
        InputActions.Disable();
        
        _currentAmmo = maxAmmo;
    }

    private void Start()
    {
        GameManager.I.RegisterPlayer(playerName, isHuman: true); 
    }

    private void OnDestroy()
    {
        _eventWrapper.WhenSelect.RemoveListener(OnGrab);
        _eventWrapper.WhenUnselect.RemoveListener(OnRelease);
        InputActions.performed -= OnUse;
    }

    private void OnGrab(PointerEvent eventData)
    {
        Debug.Log($"[Player Hand Gun] {playerName} grabbed pistol");
        InputActions.Enable();
        GameManager.I.RegisterPistolPickup(playerName);
    }

    private void OnRelease(PointerEvent eventData)
    {
        Debug.Log($"[Player Hand Gun] {playerName} released pistol");
        InputActions.Disable();
    }

    private void OnUse(InputAction.CallbackContext ctx)
    {
        Shoot();
    }

    public void Shoot() // Public for testing  - REMOVE LATER
    {
        if (_currentAmmo <= 0)
        {
            Debug.Log("[Player Hand Gun] Out of ammo!");
            return;
        }

        _currentAmmo--;
        Debug.Log($"[Player Hand Gun] {playerName} shoots! Ammo left: {_currentAmmo}");

        var bullet = Instantiate(bulletPrefab, bulletStartPosition.position, bulletStartPosition.rotation);
        bullet.GetComponent<Bullet>()?.StartBullet(bulletStartPosition.position - directionMaker.position);

        if (_currentAmmo == 0)
        {
            GameManager.I.RegisterOutOfAmmo(playerName);
        }
    }
}