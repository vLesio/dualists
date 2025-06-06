using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameMode { VRvsAI, AIvsAI }

public class GameManager : Singleton.Singleton<GameManager>
{
    public UnityEvent onGameStarted;
    public UnityEvent onGameEnded;

    public GameMode currentMode = GameMode.VRvsAI;

    private GameState _gameState = GameState.NotStarted;
    public GameState CurrentGameState => _gameState;

    private class PlayerState
    {
        public bool isHuman = false;
        public bool shieldPicked = false;
        public bool pistolPicked = false;
        public bool outOfAmmo = false;
        public bool isAlive = true;

        public bool IsReady => shieldPicked && pistolPicked;
    }

    private Dictionary<string, PlayerState> players = new Dictionary<string, PlayerState>();

    public void RegisterPlayer(string playerName, bool isHuman)
    {
        if (players.ContainsKey(playerName))
        {
            Debug.LogWarning($"[GameManager] Player {playerName} is already registered.");
            return;
        }

        players[playerName] = new PlayerState { isHuman = isHuman };
        Debug.Log($"[GameManager] Player {playerName} registered. IsHuman: {isHuman}");

        TryStartGame();
    }

    public void RegisterShieldPickup(string playerName)
    {
        if (!players.ContainsKey(playerName))
        {
            Debug.LogWarning($"[GameManager] Player {playerName} not registered yet.");
            return;
        }

        players[playerName].shieldPicked = true;
        TryStartGame();
    }

    public void RegisterPistolPickup(string playerName)
    {
        if (!players.ContainsKey(playerName))
        {
            Debug.LogWarning($"[GameManager] Player {playerName} not registered yet.");
            return;
        }

        players[playerName].pistolPicked = true;
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (_gameState.IsRunning) return;

        if (currentMode == GameMode.VRvsAI)
        {
            foreach (var pair in players)
            {
                var state = pair.Value;
                if (state.isHuman && state.IsReady)
                {
                    StartGame();
                    return;
                }
            }
        }
        else if (currentMode == GameMode.AIvsAI)
        {
            foreach (var state in players.Values)
            {
                if (!state.IsReady)
                    return;
            }

            StartGame();
        }
    }

    private void StartGame()
    {
        _gameState = GameState.Running;
        Debug.Log("[GameManager] Game Started!");
        onGameStarted.Invoke();
    }

    public void RegisterPlayerHit(string playerName)
    {
        if (_gameState.IsEnded) return;
        if (!players.ContainsKey(playerName)) return;

        players[playerName].isAlive = false;

        string winner = GetOpponent(playerName);
        _gameState = GameState.EndedWithWinner(winner);

        Debug.Log($"[GameManager] Game Over! {playerName} was hit. {winner} wins!");
        onGameEnded.Invoke();
    }

    public void RegisterOutOfAmmo(string playerName)
    {
        if (_gameState.IsEnded) return;
        if (!players.ContainsKey(playerName)) return;

        players[playerName].outOfAmmo = true;

        foreach (var state in players.Values)
        {
            if (!state.outOfAmmo)
                return;
        }

        _gameState = GameState.EndedWithTie;
        Debug.Log("[GameManager] Game Over! Draw – all players out of ammo.");
        onGameEnded.Invoke();
    }

    public bool HasGameStarted() => _gameState.IsRunning;

    private string GetOpponent(string playerName)
    {
        foreach (var name in players.Keys)
        {
            if (name != playerName)
                return name;
        }
        return "Unknown";
    }
}
