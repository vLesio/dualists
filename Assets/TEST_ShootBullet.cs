using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_ShootBullet : MonoBehaviour
{
    public HandGun pistol;

    
    public bool toggle = false;
    void Update()
    {
        if (toggle)
        {
            pistol.Shoot();
            toggle = false;
        }
    }
}
