using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSteering : MonoBehaviour, IResettable
{
    [SerializeField]
    private Transform sphereCenterPoint;

    [SerializeField][Range(0, 2)] private float sphereRadius;
    [SerializeField] [Range(0, 2)] private float minSphereRadius = 0f;
    public Vector3 GlobalPositionSphereCenterPoint => sphereCenterPoint.TransformPoint(Vector3.zero);
    public Vector2 SphereBounds => new Vector2(minSphereRadius, sphereRadius);
    
    private Hand _leftHand;
    private Hand _rightHand;

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

    public HandsObservation GetHandsObservation(Vector3 globalPositionSphereCenterPoint)
    {
        var handsObservation = new HandsObservation();
        handsObservation.Hands = new List<HandObservation>();
        handsObservation.Hands.Add(FillMissingHandObservation(_leftHand.GetHandObservation(), globalPositionSphereCenterPoint));
        handsObservation.Hands.Add(FillMissingHandObservation(_rightHand.GetHandObservation(), globalPositionSphereCenterPoint));
        return handsObservation;
    }

    private HandObservation FillMissingHandObservation(HandObservation handObservation, Vector3 globalPositionSphereCenterPoint)
    {
        handObservation.position = new Sphere3(handObservation.globalPosition, globalPositionSphereCenterPoint);      
        return handObservation;
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

        if (minSphereRadius >= sphereRadius)
        {
            Debug.LogError("[HandSteering] minSphereRadius must be less than sphereRadius. Setting minSphereRadius to 0.");
            minSphereRadius = 0f;
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