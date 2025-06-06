using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class Shield : MonoBehaviour {
    public string playerName = "PlayerVR"; // Name of the player using the shield
    
    public MeshRenderer rend;
    public Color hitColor;
    public Color pickupColor;
    public float hitIndicatorTime = 0.5f;
    public float pickupIndicatorTime = 2f;

    private float _timePassed;
    private Color _baseEmissionColor;
    private AudioSource _audioSource;
    private PointableUnityEventWrapper _eventWrapper;
    private void Start() {
        _baseEmissionColor = rend.material.GetColor("_EmissionColor");
        _audioSource = GetComponent<AudioSource>();
        _eventWrapper.WhenSelect.AddListener(ShieldPickedUp);
    }

    private void OnDestroy() {
        _eventWrapper.WhenSelect.RemoveListener(ShieldPickedUp);
    }

    private void Update() {
        _timePassed += Time.deltaTime;
        if (_timePassed >= 3f) {
            _timePassed = 0f;
            HandleBulletHit();
        }
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
        // if (!collision.gameObject.CompareTag("Bullet")) {
        //     return;
        // }

        HandleBulletHit();
    }
    
    private void ShieldPickedUp(PointerEvent pointerEvent) {
        GameManager.I.RegisterShieldPickup(playerName);
        StopAllCoroutines();
        StartCoroutine(ShowShieldAnimation(pickupColor, pickupIndicatorTime));
    }
}
