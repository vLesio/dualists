using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HandSteering : MonoBehaviour, IResettable
{
    [SerializeField]
    private Transform sphereCenterPoint;

    [SerializeField][Range(0, 2)] private float sphereRadius;
    [SerializeField] [Range(0, 2)] private float minSphereRadius = 0f;
    [SerializeField] private bool drawSphereGizmos = false;
    public Transform GlobalSphereCenterPoint => sphereCenterPoint;
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

    public HandsObservation GetHandsObservation(Transform globalSphereCenterPoint)
    {
        var handsObservation = new HandsObservation();
        handsObservation.Hands = new List<HandObservation>();
        handsObservation.Hands.Add(FillMissingHandObservation(_leftHand.GetHandObservation(), globalSphereCenterPoint));
        handsObservation.Hands.Add(FillMissingHandObservation(_rightHand.GetHandObservation(), globalSphereCenterPoint));
        return handsObservation;
    }

    private HandObservation FillMissingHandObservation(HandObservation handObservation, Transform globalSphereCenterPoint)
    {
        handObservation.position = new Sphere3(handObservation.globalPosition, globalSphereCenterPoint);      
        return handObservation;
    }
    
    void Awake()
    {
        foreach (var hand in GetComponentsInChildren<Hand>())
        {
            hand.SetBoundarySphere(GlobalSphereCenterPoint, sphereRadius);
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
    
    private void OnDrawGizmos() {
        if (!Application.isPlaying || !drawSphereGizmos) return;
        
        
        Gizmos.color = new Color(1,0,0,0.3f);
        Gizmos.DrawSphere(GlobalSphereCenterPoint.position, minSphereRadius);
        Gizmos.color = new Color(0,0,1,0.3f);
        Gizmos.DrawSphere(GlobalSphereCenterPoint.position, sphereRadius);
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