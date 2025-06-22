using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Update = UnityEngine.PlayerLoop.Update;

public enum GameMode { VRvsAI, AIvsAI }
public enum GameResult { Win, Draw, Lose }

public class PlayerState : IResettable
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

    public void Reset()
    {
        IsAlive = true;
        HasShield = !Controller.IsHuman; // AI players start with shield
        HasPistol = !Controller.IsHuman; // AI players start with pistol
        IsOutOfAmmo = false;
        Controller.Reset();
    }
}

public class GameManager : Singleton.Singleton<GameManager>
{
    // Game mode
    [FormerlySerializedAs("currentMode")] public GameMode gameMode = GameMode.VRvsAI;

    // Game state
    private readonly List<PlayerState> _playerStates = new();
    private GameState _gameState = new();
    
    // Debugging
    [SerializeField] private bool debugLogging = false;
    public float GetGameTimeProgressPercentage()
    {
        return _gameState.GameTimeProgressPercentage;
    }
    
    private void Start()
    {
        RegisterPlayers();
        StartGameIfReady();
    }

    private void Update()
    {
        bool isTimeout = _gameState.IsTimeout;
        if (isTimeout)
        {
            _gameState.End(null);
            if (debugLogging)
                Debug.Log("[GameManager] Game Over. Timeout reached.");
            ResetGame();
        }
    }
    
    private void RegisterPlayers()
    {
        _playerStates.Clear();

        foreach (Transform child in transform)
        {
            var controller = child.GetComponent<IPlayerController>();
            if (controller != null)
            {
                _playerStates.Add(new PlayerState
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
        if(!IsReadyToStart()) return;
        _gameState.Start();
        if (debugLogging)
            Debug.Log($"[GameManager] Game Started! Game Mode: {gameMode}, Players: {string.Join(", ", _playerStates.Select(p => p.Controller))}");
    }

    public void RegisterPlayerHit(IPlayerController hitPlayer)
    {
        if (!_gameState.IsRunning) return;
        var hitState = _playerStates.FirstOrDefault(p => p.Controller == hitPlayer);
        if (hitState == null || !hitState.IsAlive) return;

        hitState.IsAlive = false;

        var alive = _playerStates.Where(p => p.IsAlive).ToList();
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
        if (!_gameState.IsRunning) return;
        var state = _playerStates.FirstOrDefault(p => p.Controller == player);
        if (state == null || !state.IsAlive) return;

        state.IsOutOfAmmo = true;

        bool allAliveOutOfAmmo = _playerStates
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
        PropagateGameResult();
        BulletManager.I.Reset();

        foreach (var state in _playerStates)
        {
            state.Reset();
        }
        _gameState = new GameState();

        StartGameIfReady();
    }

    private void PropagateGameResult()
    {
        if (!_gameState.IsEnded) return;
        
        foreach (var state in _playerStates)
        {
            if (_gameState.IsTie) {
                state.Controller.EndGame(GameResult.Draw);
            } else if (_gameState.Winner == state.Controller) {
                state.Controller.EndGame(GameResult.Win);
            }
            else {
                state.Controller.EndGame(GameResult.Lose);
            }
        }
    }

    public bool IsRunning() => _gameState.IsRunning;

    public void RegisterPistolPickup(VRPlayerController vrPlayer)
    {
        if (_gameState.IsRunning) return;
        var state = _playerStates.FirstOrDefault(p => ReferenceEquals(p.Controller, vrPlayer));
        if (state == null) return;

        state.HasPistol = true;
        if (debugLogging)
            Debug.Log("[GameManager] Registered pistol pickup for VR player.");
        
        StartGameIfReady();
    }
    public void RegisterShieldPickup(VRPlayerController vrPlayer)
    {
        if (_gameState.IsRunning) return;
        var state = _playerStates.FirstOrDefault(p => ReferenceEquals(p.Controller, vrPlayer));
        if (state == null) return;
        
        state.HasShield = true;
        if (debugLogging)
            Debug.Log("[GameManager] Registered shield pickup for VR player.");

        StartGameIfReady();
    }
    
    private bool IsReadyToStart()
    {
        // Check if the game is already running or if there are not enough players
        if (_gameState.IsRunning || _playerStates.Count < 2) return false;

        // Too many players
        if (_playerStates.Count > 2)
        {
            Debug.LogWarning("[GameManager] More than 2 players detected. Only the first two will be used.");
            _playerStates.RemoveRange(2, _playerStates.Count - 2);
        }
        
        // Based on the game mode...
        if (gameMode == GameMode.AIvsAI)
        {
            // AI vs AI mode requires only AI players
            if (_playerStates.Any(p => p.Controller.IsHuman))
            {
                Debug.LogError("[GameManager] AIvsAI mode requires only AI players. At least one human player detected.");
                return false;
            }

            return true;
        }
        
        if (gameMode == GameMode.VRvsAI)
        {
            bool exactlyOneHumanAndOneAI = _playerStates.Count(p => p.Controller.IsHuman) == 1
                                           && _playerStates.Count(p => !p.Controller.IsHuman) == 1;
            // VR vs AI mode requires exactly one human player and one AI player
            if (!exactlyOneHumanAndOneAI)
            {
                Debug.LogError("[GameManager] VRvsAI mode requires exactly one human player and one AI player. Current players: " + string.Join(", ", _playerStates.Select(p => p.Controller)));
                return false;
            }
            
            // Ensure human player grabbed the pistol and shield
            if (!_playerStates.Any(p => p.Controller.IsHuman && p.HasPistol && p.HasShield))
            {
                if (debugLogging)
                    Debug.Log("[GameManager] Human player must have a pistol and shield to start the game.");
                return false;
            }
        }

        return true;
    }
    
    [CanBeNull]
    public GameObject GetOpponent(IPlayerController player)
    {
        if(_playerStates.Count() < 2 || _playerStates.Count() > 2 || _playerStates.All(p => p.Controller != player))
        {
            Debug.LogError("[GameManager] Cannot find opponent for player: " + player);
            return null;
        }

        var controller = _playerStates.FirstOrDefault(p => p.Controller != player)?.Controller;
        if (controller is MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject;
        }

        return null;
    }
}
