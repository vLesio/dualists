using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HandGun))]
public class PlayerHandGun : MonoBehaviour
{
    [Header("References")]
    public InputAction inputActions;
    private PointableUnityEventWrapper _eventWrapper;
    private HandGun _handgun;

    private void Awake() {
        _handgun = GetComponent<HandGun>();
        _eventWrapper = GetComponent<PointableUnityEventWrapper>();
        _eventWrapper.WhenSelect.AddListener(OnGrab);
        _eventWrapper.WhenUnselect.AddListener(OnRelease);
        inputActions.performed += OnUse;
        inputActions.Disable();
    }

    private void OnDestroy() {
        _eventWrapper.WhenSelect.RemoveListener(OnGrab);
        _eventWrapper.WhenUnselect.RemoveListener(OnRelease);
        inputActions.performed -= OnUse;
    }

    private void OnGrab(PointerEvent eventData)
    {
        inputActions.Enable();
        GameManager.I.RegisterPistolPickup();
    }

    private void OnRelease(PointerEvent eventData)
    {
        inputActions.Disable();
    }

    private void OnUse(InputAction.CallbackContext ctx)
    {
        _handgun.Shoot();
    }
}