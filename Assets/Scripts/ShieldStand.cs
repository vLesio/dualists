using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldStand : MonoBehaviour
{
    private void Start() {
        GameManager.I.onShieldPickedUp.AddListener(HandleShieldPickup);
    }

    private void HandleShieldPickup() {
        GameManager.I.onShieldPickedUp.RemoveListener(HandleShieldPickup);
        Destroy(gameObject);
    }
}
