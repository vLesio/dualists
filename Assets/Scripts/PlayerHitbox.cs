using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public string playerName = "PlayerVR";

    private bool _alreadyHit = false;

    public void OnHit()
    {
        if (_alreadyHit) return;

        _alreadyHit = true;
        Debug.Log($"[Player Hitbox] {playerName} was hit by a bullet!");
        GameManager.I.RegisterPlayerHit(playerName);
    }
}
