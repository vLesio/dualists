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

    public void OnTriggerEnter(Collider other) {
        DestroyBullet();
    }

    public void DestroyBullet() {
        Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(this);
    }
}
