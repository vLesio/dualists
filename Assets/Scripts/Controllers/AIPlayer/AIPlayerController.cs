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
    
    
    // Neural Network Actions Parsing
    
    // All continuous actions are in the range [-1, 1]
    public void ParseNNActions(ActionBuffers actions)
    {
        var continuous = actions.ContinuousActions;
        var discrete = actions.DiscreteActions;
        if (continuous.Length < 18)
        {
            Debug.LogError($"AIPlayerController: Continuous actions are not set up correctly. Should be 18, but got {continuous.Length}");
            return;
        }
        if(discrete.Length != 1)
        {
            Debug.LogError($"AIPlayerController: Discrete actions are not set up correctly. Should be 1, but got {actions.DiscreteActions.Length}");
            return;
        }
        
        HandsDesiredActions handsDesiredActions = new HandsDesiredActions();   
        
        for(int i = 0; i < 2; i++)
        {
            int offset = i * 9;
            HandSide handSide = (i == 0) ? HandSide.right : HandSide.left;
            var desiredActions = new HandDesiredActions
            {
                handSide = handSide,
                moveSpeed = NormalizeTo01(continuous[0 + offset]),
                rotationSpeed = NormalizeTo01(continuous[1 + offset]),
                position = NormalizedFloat3ToSphere(
                    continuous[2 + offset], 
                    continuous[3 + offset], 
                    continuous[4 + offset]),
                rotation = NormalizedFloat4ToQuaternion(
                    continuous[5 + offset], 
                    continuous[6 + offset], 
                    continuous[7 + offset], 
                    continuous[8 + offset])
            };
            handsDesiredActions.Hands.Add(desiredActions);
        }
        
        bool shoot = ShouldShoot(discrete);
        
        ApplyActions(handsDesiredActions, shoot);
    }

    private Sphere3 NormalizedFloat3ToSphere(float theta, float phi, float radius)
    {
        float minR = handSteering.SphereBounds.x;
        float maxR = handSteering.SphereBounds.y;
        Vector3 center = handSteering.GlobalPositionSphereCenterPoint;
        return Sphere3.FromNormalized(radius, theta, phi, center, maxR, minR);
    }

    private Quaternion NormalizedFloat4ToQuaternion(float x, float y, float z, float w)
    {
        return Quaternion.Normalize(
            new Quaternion(x, y, z, w));
    }
    
    private float NormalizeTo01(float x) => (x + 1f) / 2f;
    
    private bool ShouldShoot(ActionSegment<int> discrete)
    {
        return discrete[0] == 1; // Assuming the first discrete action is for shooting
    }
    
    private void ApplyActions(HandsDesiredActions handsDesiredActions, bool shoot)
    {
        handSteering.PropagateHandsActions(handsDesiredActions);
        if (shoot)
        {
            handGun.Shoot();
        }
    }
}
