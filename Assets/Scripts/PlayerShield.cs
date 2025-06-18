

using System;
using Oculus.Interaction;
using UnityEngine;

public class PlayerShield : MonoBehaviour {
    
    private PointableUnityEventWrapper _eventWrapper;
    private Shield _shield;
    
    private VRPlayerController _owner;

    private void Awake()
    {
        _owner = GetComponentInParent<VRPlayerController>();
        if (_owner == null)
            Debug.LogWarning("[PlayerShield] No VRPlayerController found in parent! The shield will not register pickups in the game.");
    }

    private void Start() {
        _shield = GetComponent<Shield>();
        _eventWrapper.WhenSelect.AddListener(ShieldPickedUp);
    }
    
    private void OnDestroy() {
        _eventWrapper.WhenSelect.RemoveListener(ShieldPickedUp);
    }
    
    private void ShieldPickedUp(PointerEvent pointerEvent) {
        if (_owner != null) // Notifying the controller that the shield was picked up
        {
            _owner.OnShieldPickup();
        }
        _shield.HandleShieldPickedUp();
    }
}