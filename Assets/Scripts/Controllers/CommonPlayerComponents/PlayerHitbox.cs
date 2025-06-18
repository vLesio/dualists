using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerHitbox : MonoBehaviour, IResettable
{
    private bool _alreadyHit = false;
    private IPlayerController _controller;

    private void Awake()
    {
        _controller = GetComponentInParent<IPlayerController>();
        if (_controller == null)
        {
            Debug.LogError("PlayerHitbox must be a child of IPlayerController!");
        }
    }

    public void OnHit()
    {
        if (_alreadyHit || !GameManager.I.IsRunning()) return;

        _alreadyHit = true;

        if (_controller != null)
        {
            _controller.OnHit();
        }
    }
    
    public void Reset()
    {
        _alreadyHit = false;
    }
}
