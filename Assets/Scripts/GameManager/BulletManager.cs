using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : Singleton.Singleton<BulletManager>, IResettable
{
    [SerializeField] private bool debugLogging = false;
    
    public void Reset()
    {
        var bulletsToDestroy = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.GetComponent<Bullet>() != null)
            {
                bulletsToDestroy.Add(child.gameObject);
            }
        }

        foreach (var bullet in bulletsToDestroy)
        {
            Destroy(bullet);
        }
        
        if (debugLogging)
        {
            Debug.Log($"[BulletManager] Reset called, {bulletsToDestroy.Count} bullets  destroyed.");
        }
    }
}
