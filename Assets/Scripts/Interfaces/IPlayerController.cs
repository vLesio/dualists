using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerController : IResettable
{
    bool IsHuman { get; }
    bool IsAlive { get; }
    bool IsOutOfAmmo { get; }
    void OnHit();
    void OnOutOfAmmo();
    void EndGame(GameResult gameResult);
}
