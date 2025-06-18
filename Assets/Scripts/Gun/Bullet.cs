using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float speed = 1f;
    private Vector3 _direction = Vector3.zero;
    private bool _shouldPlay = false;
    [SerializeField] private float maxDistance = 1000f;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject bloodEffect;
    
    private Rigidbody _rb;
    private Vector3 startPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("[Bullet] Rigidbody component is missing!");
        }
    }

    private void FixedUpdate()
    {
        if (_shouldPlay)
        {
            _rb.MovePosition(_rb.position + _direction * (speed * Time.fixedDeltaTime));
        }

        if (Vector3.Distance(transform.position, startPosition) > maxDistance)
        {
            DestroyBullet();
        }
    }

    public void StartBullet(Vector3 direction)
    {
        _direction = direction.normalized;
        _shouldPlay = true;
        startPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHitbox hitbox = other.GetComponent<PlayerHitbox>();
        if (hitbox != null)
        {
            hitbox.OnHit();
        }

        Vector3 hitPoint = transform.position;

        HandleRaycastAndInstantiateEffect(other, ref hitPoint, out var effectRotation);
        InstantiateSfxEffect(hitPoint, effectRotation, other.tag);

        DestroyBullet();
    }
    
    private void HandleRaycastAndInstantiateEffect(Collider other, ref Vector3 hitPoint, out Quaternion rotation)
    {
        if (Physics.Raycast(transform.position - _direction * 0.1f, _direction, out RaycastHit hit, 1f))
        {
            hitPoint = hit.point;
            Vector3 reflected = Vector3.Reflect(_direction, hit.normal);
            rotation = Quaternion.LookRotation(reflected);
        }
        else
        {
            Debug.LogWarning("[Bullet] Raycast failed, fallback to approximate hit point");
            rotation = Quaternion.LookRotation(Vector3.Reflect(_direction, -_direction));
        }
    }

    private void InstantiateSfxEffect(Vector3 position, Quaternion rotation, string colliderTag)
    {
        GameObject effect = null;
        switch (colliderTag)
        {
            case "Dualist":
                effect = Instantiate(bloodEffect, position, rotation);
                break;
            default:
                effect = Instantiate(hitEffect, position, rotation);
                break;
        }
        if (effect != null)
        {
            Destroy(effect, 1f);
        }
    }
    
    public void DestroyBullet()
    {
        Destroy(gameObject);
    }
    
}
