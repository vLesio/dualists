using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton.Singleton<GameManager> {
    public UnityEvent onGameStarted;
    public UnityEvent onGameEnded;
    public UnityEvent onShieldPickedUp; // Will only fire once
    public UnityEvent onPistolPickedUp; // Will only fire once

    private bool _shieldPickedUp = false;
    private bool _pistolPickedUp = false;
    private bool _gameStarted = false;
    
    public void RegisterShieldPickup() {
        _shieldPickedUp = true;
        onShieldPickedUp.Invoke();
    }

    public void RegisterPistolPickup() {
        _pistolPickedUp = true;
        onPistolPickedUp.Invoke();
    }
    
    private void StartGame() {
        onGameStarted.Invoke();
    }

    private void EndGame() {
        onGameEnded.Invoke();
    }
}
