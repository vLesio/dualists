using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour {
    public float speed = 1f;
    private Vector3 _direction = Vector3.zero;
    private bool _shouldPlay = false;
    [SerializeField] private float maxDistance = 1000f;
    [SerializeField] private GameObject hitEffect;
    private Vector3 startPosition;

    private void Update() {
        if (_shouldPlay) {
            transform.Translate(_direction * (speed * Time.deltaTime), Space.World);
        }
        if(Vector3.Distance(transform.position, startPosition) > maxDistance) DestroyBullet();
    }

    public void StartBullet(Vector3 direction) {
        direction = direction.normalized;
        _direction = direction;
        _shouldPlay = true;
        startPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 hitPoint = transform.position;

        if (Physics.Raycast(transform.position - _direction * 0.1f, _direction, out RaycastHit hit, 1f))
        {
            hitPoint = hit.point;

            Vector3 incoming = _direction;
            Vector3 normal = hit.normal;
            Vector3 reflected = Vector3.Reflect(incoming, normal);

            Quaternion rotation = Quaternion.LookRotation(reflected);

            Instantiate(hitEffect, hitPoint, rotation);
        }
        else
        {
            Debug.LogWarning("[Bullet] Raycast failed, fallback to approximate hit point");

            Vector3 fallbackReflected = Vector3.Reflect(_direction, -_direction);
            Quaternion fallbackRot = Quaternion.LookRotation(fallbackReflected);

            Instantiate(hitEffect, hitPoint, fallbackRot);
        }
        DestroyBullet();
    }

    public void DestroyBullet() {
        Destroy(gameObject);
    }
    
    // TODO : REMOVE THAT
    private void Start()
    {
        StartBullet(-transform.up);
    }
}
