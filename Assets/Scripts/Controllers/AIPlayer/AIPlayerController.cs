using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(HandSteering))]
[RequireComponent(typeof(HandGun))]
[RequireComponent(typeof(PlayerHitbox))]
public class AIPlayerController : MonoBehaviour, IPlayerController
{
    // Components
    [Header("Components")]
    public HandSteering handSteering;
    public HandGun handGun;
    public PlayerHitbox playerHitbox;

    public bool IsHuman => false;

    // State
    private bool _isAlive = true;
    public bool IsAlive => _isAlive;
    private bool _isOutOfAmmo = false;
    public bool IsOutOfAmmo => _isOutOfAmmo;
    
    // Debug
    [SerializeField] private bool debugLogging = false;
    
    public void ParseNNActions(ActionBuffers actions)
    {
        
    }

    public void OnHit()
    {
        if(!_isAlive || !GameManager.I.IsRunning()) return;
        
        _isAlive = false;
        if(debugLogging)
            Debug.Log($"[AI Player Controller] Player: {this.ToString()} : Hit received!");
        
        GameManager.I.RegisterPlayerHit(this);
    }

    public void OnOutOfAmmo()
    {
        if (_isOutOfAmmo || !_isAlive) return;
        
        if (debugLogging)
            Debug.Log($"[AI Player Controller] Player: {this.ToString()} : Out of ammo!");
        _isOutOfAmmo = true;
        GameManager.I.RegisterPlayerOutOfAmmo(this);
    }

    public void Reset()
    {
        _isAlive = true;
        _isOutOfAmmo = false;
        playerHitbox.Reset();
        handSteering.Reset();
        handGun.Reset();
        if (debugLogging)
            Debug.Log($"[AI Player Controller] Player: {this.ToString()} : Reset called, player is alive and has ammo.");
    }
    
    public override string ToString()
    {
        return $"AI[{gameObject.name}]";
    }
}
