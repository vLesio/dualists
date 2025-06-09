using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class Shield : MonoBehaviour {
    public MeshRenderer rend;
    public Color hitColor;
    public float hitIndicatorTime = 0.5f;
    public Color pickupColor;
    public float pickupIndicatorTime = 2f;

    private float _timePassed;
    private Color _baseEmissionColor;
    private AudioSource _audioSource;
    
    private void Start() {
        _baseEmissionColor = rend.material.GetColor("_EmissionColor");
        _audioSource = GetComponent<AudioSource>();
    }

    public void HandleShieldPickedUp() {
        StopAllCoroutines();
        StartCoroutine(ShowShieldAnimation(pickupColor, pickupIndicatorTime));
    }

    IEnumerator ShowShieldAnimation(Color color, float duration) {
        var timePassed = 0f;
        while (timePassed <= duration) {
            timePassed += Time.deltaTime;
            var lerpedColor = Color.Lerp(color, _baseEmissionColor, timePassed / duration);
            rend.material.SetColor("_EmissionColor", lerpedColor);
            yield return null;
        }
        rend.material.SetColor("_EmissionColor", _baseEmissionColor);
    }

    void HandleBulletHit() {
        _audioSource.Play();
        StopAllCoroutines();
        StartCoroutine(ShowShieldAnimation(hitColor, hitIndicatorTime));
    }

    void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.CompareTag("Bullet")) {
            return;
        }

        HandleBulletHit();
    }
}
