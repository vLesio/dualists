using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPlayerController : MonoBehaviour, IPlayerController
{
    // TODO: Implement Reset functionality for VRPlayer, especially how the VR player's gun and shield are handled.
    
    
    // Components
    [Header("Components")]
    public PlayerHitbox playerHitbox;
    public HandGun handGun;
    

    public bool IsHuman => true;

    // State
    private bool _isAlive = true;
    public bool IsAlive => _isAlive;
    private bool _isOutOfAmmo = false;
    public bool IsOutOfAmmo => _isOutOfAmmo;
    
    // Debug
    [SerializeField] private bool debugLogging = false;
    
    public void OnHit()
    {
        if(!_isAlive || !GameManager.I.IsRunning()) return;
        
        _isAlive = false;
        if(debugLogging)
            Debug.Log($"[VR Player Controller] Player: {this.ToString()} : Hit received!");
        
        GameManager.I.RegisterPlayerHit(this);
    }

    public void OnOutOfAmmo()
    {
        if (_isOutOfAmmo || !_isAlive) return;
        
        if (debugLogging)
            Debug.Log($"[VR Player Controller] Player: {this.ToString()} : Out of ammo!");
        _isOutOfAmmo = true;
        GameManager.I.RegisterPlayerOutOfAmmo(this);
    }
    
    public void OnPistolPickup()
    {
        if (!_isAlive) return;
        
        if (debugLogging)
            Debug.Log($"[VR Player Controller] Player: {this.ToString()} : Pistol picked up!");

        GameManager.I.RegisterPistolPickup(this);
    }
    
    public void OnShieldPickup()
    {
        if (!_isAlive) return;
        
        if (debugLogging)
            Debug.Log($"[VR Player Controller] Player: {this.ToString()} : Shield picked up!");

        GameManager.I.RegisterShieldPickup(this);
    }

    public void Reset()
    {
        _isAlive = true;
        _isOutOfAmmo = false;
        playerHitbox.Reset();
        handGun.Reset();
        if (debugLogging)
            Debug.Log($"[VR Player Controller] Player: {this.ToString()} : Reset called, player is alive and has ammo.");
    }
    
    public override string ToString()
    {
        return $"VR[{gameObject.name}]";
    }
}
