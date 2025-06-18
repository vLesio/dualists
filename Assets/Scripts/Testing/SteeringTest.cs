using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AgentOutput
{
    
}

[RequireComponent(typeof(HandSteering))]
public class SteeringTest : MonoBehaviour
{
    [SerializeField] private GameObject leftHandTrigger;
    [SerializeField] private GameObject rightHandTrigger;
    [SerializeField] private bool sendDirectMovementInfo;
    [SerializeField] private float sendingIntervalInSeconds;
    private float _lastSendTime;
    private HandSteering _handSteering;

    private void Awake()
    {
        _handSteering = GetComponent<HandSteering>();
    }

    void Update()
    {
        if (sendDirectMovementInfo)
        {
            if (Time.time - _lastSendTime >= sendingIntervalInSeconds)
            {
                PrepareAndSendHandsActions();
                _lastSendTime = Time.time;
            }
        }
    }

    public AgentOutput GetTestAgentOutput()
    {
        return new AgentOutput();
    }

    private void PrepareAndSendHandsActions()
    {
        var handsActions = PrepareHandsActions();
        SendHandsActions(handsActions);
    }

    private HandsDesiredActions PrepareHandsActions()
    {
        var handsDesiredActions = new HandsDesiredActions();
        handsDesiredActions.Hands = new List<HandDesiredActions>() {
            PrepareLeftHandActions(), 
            PrepareRightHandActions()
        };
        return handsDesiredActions;
    }

    private HandDesiredActions PrepareLeftHandActions()
    {        
        var handAction = GetDummyHandActions(HandSide.left);
        var sphere = new Sphere3();
        sphere.FromGlobalVector3(leftHandTrigger.transform.position, _handSteering.GlobalPositionSphereCenterPoint);
        handAction.position = sphere;
        handAction.rotation = leftHandTrigger.transform.rotation;
        return handAction;
    }    
    private HandDesiredActions PrepareRightHandActions()
    {
        var handAction = GetDummyHandActions(HandSide.right);
        var sphere = new Sphere3();
        sphere.FromGlobalVector3(rightHandTrigger.transform.position, _handSteering.GlobalPositionSphereCenterPoint);
        handAction.position = sphere;
        handAction.rotation = rightHandTrigger.transform.rotation;
        return handAction;
    }

    private HandDesiredActions GetDummyHandActions(HandSide handSide)
    {
        var handAction = new HandDesiredActions();
        handAction.handSide = handSide;
        handAction.moveSpeed = 1f;
        handAction.rotationSpeed = 1f;
        return handAction;
    }
    
    private void SendHandsActions(HandsDesiredActions handsActions)
    {
        _handSteering.PropagateHandsActions(handsActions);
    }
}
