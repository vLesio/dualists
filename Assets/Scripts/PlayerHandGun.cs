using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerHandGun : MonoBehaviour {
    private PointableUnityEventWrapper _eventWrapper;
    public InputAction InputActions;
    public GameObject bulletPrefab;
    public Transform bulletStartPosition;
    public Transform directionMaker;

    private void Awake() {
        _eventWrapper = GetComponent<PointableUnityEventWrapper>();
        _eventWrapper.WhenSelect.AddListener(OnGrab);
        _eventWrapper.WhenUnselect.AddListener(OnRelease);
        InputActions.performed += OnUse;
        InputActions.Disable();
    }

    private void OnDestroy() {
        _eventWrapper.WhenSelect.RemoveListener(OnGrab);
        _eventWrapper.WhenUnselect.RemoveListener(OnRelease);
    }

    private void OnGrab(PointerEvent eventData) {
        Debug.Log("Grabbed");
        InputActions.Enable();
        GameManager.I.RegisterPistolPickup();
    }

    private void OnRelease(PointerEvent eventData) {
        Debug.Log("Released");
        InputActions.Disable();
    }

    private void OnUse(InputAction.CallbackContext ctx) {
        Shoot();
    }

    private void Shoot() {
        Debug.Log("Shoot!");
        Instantiate(bulletPrefab, bulletStartPosition.position, bulletStartPosition.rotation)
            .GetComponent<Bullet>()
            .StartBullet(bulletStartPosition.position - directionMaker.position);
    }
}
