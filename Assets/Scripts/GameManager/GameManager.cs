using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum GameMode { VRvsAI, AIvsAI }

public class PlayerState
{
    public IPlayerController Controller;
    public bool IsAlive = true;
    public bool HasShield = false;
    public bool HasPistol = false;
    public bool IsOutOfAmmo = false;

    public override string ToString()
    {
        return $"{Controller} | Alive: {IsAlive}, Ammo: {!IsOutOfAmmo}, Pistol: {HasPistol}, Shield: {HasShield}";
    }
}

public class GameManager : Singleton.Singleton<GameManager>
{
    // Game mode
    public GameMode currentMode = GameMode.VRvsAI;

    // Game state
    private readonly List<PlayerState> playerStates = new();
    private GameState _gameState = new();
    
    // Debugging
    [SerializeField] private bool debugLogging = false;
    
    private void Start()
    {
        RegisterPlayers();
        StartGameIfReady();
    }

    private void RegisterPlayers()
    {
        playerStates.Clear();

        foreach (Transform child in transform)
        {
            var controller = child.GetComponent<IPlayerController>();
            if (controller != null)
            {
                playerStates.Add(new PlayerState
                {
                    Controller = controller,
                    IsAlive = true,
                    HasShield = !controller.IsHuman, // AI players start with shield
                    HasPistol = !controller.IsHuman, // AI players start with pistol
                });
            }
        }
    }

    private void StartGameIfReady()
    {
        if (_gameState.IsRunning || playerStates.Count < 2) return;

        if (playerStates.Count > 2)
        {
            Debug.LogWarning("[GameManager] More than 2 players detected. Only the first two will be used.");
            playerStates.RemoveRange(2, playerStates.Count - 2);
        }

        if (currentMode == GameMode.AIvsAI)
        {
            _gameState.Start();
            if (debugLogging)
                Debug.Log($"[GameManager] Game Started! Game Mode: {currentMode}, Players: {string.Join(", ", playerStates.Select(p => p.Controller))}");
        }

        // TODO: Implement VRvsAI mode
        if (currentMode == GameMode.VRvsAI)
        {
            Debug.LogWarning("[GameManager] VRvsAI mode is not implemented yet.");
            return;
        }
    }

    public void RegisterPlayerHit(IPlayerController hitPlayer)
    {
        if (!_gameState.IsRunning) return;

        var hitState = playerStates.FirstOrDefault(p => p.Controller == hitPlayer);
        if (hitState == null || !hitState.IsAlive) return;

        hitState.IsAlive = false;

        var alive = playerStates.Where(p => p.IsAlive).ToList();
        if (alive.Count == 1)
        {
            _gameState.End(alive[0].Controller);
            if(debugLogging)
                Debug.Log($"[GameManager] Game Over. Winner: {alive[0].Controller}");
        }
        else if (alive.Count == 0)
        {
            _gameState.End(null);
            if(debugLogging)
                Debug.Log("[GameManager] Game Over. Tie (both died)");
        }  
        else
        {
            Debug.LogError("[GameManager] More than one player is still alive after a hit. This should not happen.");
        }

        ResetGame();
    }

    public void RegisterPlayerOutOfAmmo(IPlayerController player)
    {
        var state = playerStates.FirstOrDefault(p => p.Controller == player);
        if (state == null || !state.IsAlive) return;

        state.IsOutOfAmmo = true;

        bool allAliveOutOfAmmo = playerStates
            .Where(p => p.IsAlive)
            .All(p => p.IsOutOfAmmo);

        if (allAliveOutOfAmmo)
        {
            _gameState.End(null);
            if(debugLogging)
                Debug.Log("[GameManager] Game Over. Tie (all alive players out of ammo)");
            ResetGame();
        }
    }

    private void ResetGame()
    {
        BulletManager.I.Reset();

        foreach (var state in playerStates)
        {
            state.IsAlive = true;
            state.HasShield = false;
            state.HasPistol = false;
            state.IsOutOfAmmo = false;
            state.Controller.Reset();
        }
        _gameState = new GameState();

        StartGameIfReady();
    }

    public bool IsRunning() => _gameState.IsRunning;

    public void RegisterPistolPickup()
    {
        /*
        var state = playerStates.FirstOrDefault()?;
        if (state != null)
        {
            state.HasPistol = true;
        }*/
    }

    public void RegisterShieldPickup()
    {
        /*
        var state = playerStates.FirstOrDefault()?;
        if (state != null)
        {
            state.HasShield = true;
        }*/
    }
}
