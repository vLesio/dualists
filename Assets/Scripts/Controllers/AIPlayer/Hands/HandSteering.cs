using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSteering : MonoBehaviour, IResettable
{
    [SerializeField]
    private Transform sphereCenterPoint;

    [SerializeField][Range(0, 10)] private float sphereRadius;
    public Vector3 GlobalPositionSphereCenterPoint => sphereCenterPoint.TransformPoint(Vector3.zero);
    
    private Hand _leftHand;
    private Sphere3 _leftHandSpherePosition;
    private Hand _rightHand;
    private Sphere3 _rightHandSpherePosition;

    public void PropagateHandsActions(HandsDesiredActions handsActions) {
        foreach (var handAction in handsActions.Hands) {
            if (handAction.handSide == HandSide.left) {
                _leftHand.PropagateDesiredAction(handAction);
            }
            if (handAction.handSide == HandSide.right) {
                _rightHand.PropagateDesiredAction(handAction);
            }
        }
    }

    public HandsObservation GetHandsObservation()
    {
        var handsObservation = new HandsObservation();
        handsObservation.Hands = new List<HandObservation>();
        handsObservation.Hands.Add(FillMissingHandObservation(_leftHand.GetHandObservation()));
        handsObservation.Hands.Add(FillMissingHandObservation(_rightHand.GetHandObservation()));
        return handsObservation;
    }

    private HandObservation FillMissingHandObservation(HandObservation handObservation)
    {
        if(handObservation.handSide == HandSide.left)
            handObservation.position = _leftHandSpherePosition;        
        if(handObservation.handSide == HandSide.right)
            handObservation.position = _rightHandSpherePosition;
        return handObservation;
    }
    void Update()
    {
    }
    
    void Awake()
    {
        foreach (var hand in GetComponentsInChildren<Hand>())
        {
            hand.SetBoundarySphere(GlobalPositionSphereCenterPoint, sphereRadius);
            if (hand.HandSide == HandSide.left) {
                _leftHand = hand;
            }
            if (hand.HandSide == HandSide.right) {
                _rightHand = hand;
            }
        }
    }

    public void Reset()
    {
        _leftHand.Reset();
        _rightHand.Reset();
    }
}
public struct HandsObservation
{
    public List<HandObservation> Hands;
}

public struct HandsDesiredActions
{
    public List<HandDesiredActions> Hands;
}