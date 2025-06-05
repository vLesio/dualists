using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    public MeshRenderer rend;
    public Color hitColor;
    public float hitIndicatorTime = 0.5f;

    private float _timePassed;
    private Color _baseEmissionColor;
    private AudioSource _audioSource;
    private void Start() {
        _baseEmissionColor = rend.material.GetColor("_EmissionColor");
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        _timePassed += Time.deltaTime;
        if (_timePassed >= 3f) {
            _timePassed = 0f;
            HandleBulletHit();
        }
    }

    IEnumerator ShowShieldHit() {
        var timePassed = 0f;
        while (timePassed <= hitIndicatorTime) {
            timePassed += Time.deltaTime;
            var lerpedColor = Color.Lerp(hitColor, _baseEmissionColor, timePassed / hitIndicatorTime);
            rend.material.SetColor("_EmissionColor", lerpedColor);
            yield return null;
        }
        rend.material.SetColor("_EmissionColor", _baseEmissionColor);
    }

    void HandleBulletHit() {
        _audioSource.Play();
        StopAllCoroutines();
        StartCoroutine(ShowShieldHit());
    }

    void OnCollisionEnter(Collision collision) {
        // if (!collision.gameObject.CompareTag("Bullet")) {
        //     return;
        // }

        HandleBulletHit();
    }
}
