using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Hand : MonoBehaviour, IResettable
{
    private Rigidbody _rb;
    [SerializeField]
    private HandSide handSide;
    public HandSide HandSide => handSide;

    [SerializeField]
    private float moveBaseSpeed;
    [SerializeField]
    private float rotationBaseSpeed;
    [SerializeField] private ForceMode rotationForceMode;
    [SerializeField] private ForceMode movementForceMode;

    [SerializeField]
    private float boundarySphereRadius;
    [SerializeField]
    private Vector3 globalPositionBoundarySphereCenter;
    
    [SerializeField] private bool drawGizmo = false;
    
    // Start variables
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    
    private void Start()
    {
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
    }
    
    public void Reset()
    {
        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
    }
    
    public void SetBoundarySphere(Vector3 globalPositionCenter, float radius) {
        globalPositionBoundarySphereCenter = globalPositionCenter;
        boundarySphereRadius = radius;
    }

    public void PropagateDesiredAction(HandDesiredActions actions)
    {
        MoveToPosition(actions.position.ToGlobalVector3(), actions.moveSpeed);  
        AddTorqueToRotation(actions.rotation, actions.rotationSpeed);
        ActIfHandIsOutOfBounds();
    }
    private void MoveToPosition(Vector3 globalPosition, float speed) {
        speed = NormalizeSpeed(speed);
        _rb.AddForce(CalculateMoveDirection(globalPosition) * (speed * moveBaseSpeed), ForceMode.Force);
    }
    private void AddTorqueToRotation(Quaternion desiredRotation, float speed) {
        Quaternion currentRotation = transform.rotation; 
        Quaternion targetRotation = desiredRotation; 
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(currentRotation);

        rotationDifference.ToAngleAxis(out var rotationAngle, out var rotationAxis);

        if (rotationAngle > 180f) {
            rotationAngle -= 360f;
        }
        
        _rb.AddTorque(rotationAxis * (rotationAngle * speed * rotationBaseSpeed));
    }
    public HandObservation GetHandObservation()
    {
        var handObservation = new HandObservation();
        handObservation.handSide = handSide;
        handObservation.rotation = transform.rotation;
        handObservation.velocity = _rb.velocity;
        handObservation.angularVelocity = _rb.angularVelocity;
        handObservation.globalPosition = transform.position;
        return handObservation;
    }
    private float NormalizeSpeed(float speed) {
        return math.clamp(speed, 0, 1);
    }
    private Vector3 CalculateMoveDirection(Vector3 v) {
        Vector3 dir = v - transform.position;
        return dir.normalized;
    }
    private void Update() {
        ActIfHandIsOutOfBounds();
    }    
    private void FixedUpdate() {
        ActIfHandIsOutOfBounds();
    }
    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    private void ActIfHandIsOutOfBounds()
    {
        if (Vector3.Distance(transform.position, globalPositionBoundarySphereCenter) > boundarySphereRadius)
        {
            InverseVelocity();
        }
    }
    private void InverseVelocity()
    {
        var nextPost = transform.position + _rb.velocity;
        if(Vector3.Distance(nextPost, globalPositionBoundarySphereCenter) > Vector3.Distance(transform.position, globalPositionBoundarySphereCenter))
            _rb.velocity = -_rb.velocity;
    }
    private void OnDrawGizmos() {
        if (!Application.isPlaying || !drawGizmo) return;
        Gizmos.color = new Color(0,0,1,0.2f);
        Gizmos.DrawSphere(globalPositionBoundarySphereCenter, boundarySphereRadius);
    }
}
public enum HandSide
{
    left,
    right
}

public struct HandObservation
{
    public HandSide handSide;
    public Sphere3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public Vector3 globalPosition;
}

public struct HandDesiredActions
{
    public HandSide handSide;
    public Sphere3 position;
    public float moveSpeed;
    public Quaternion rotation;
    public float rotationSpeed;
}