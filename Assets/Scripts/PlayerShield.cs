

using System;
using Oculus.Interaction;
using UnityEngine;

public class PlayerShield : MonoBehaviour {
    
    private PointableUnityEventWrapper _eventWrapper;
    private Shield _shield;

    private void Start() {
        _shield = GetComponent<Shield>();
        _eventWrapper.WhenSelect.AddListener(ShieldPickedUp);
    }
    
    private void OnDestroy() {
        _eventWrapper.WhenSelect.RemoveListener(ShieldPickedUp);
    }
    
    private void ShieldPickedUp(PointerEvent pointerEvent) {
        GameManager.I.RegisterShieldPickup();
        _shield.HandleShieldPickedUp();
    }
}