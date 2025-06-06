using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_GameManager : MonoBehaviour
{
    public PlayerHandGun playerHandGun;
    void Start()
    {

        
        GameManager.I.RegisterPistolPickup("PlayerVR");
        GameManager.I.RegisterShieldPickup("PlayerVR");

        playerHandGun.Shoot();
    }

 
    void Update()
    {
        
    }
}
