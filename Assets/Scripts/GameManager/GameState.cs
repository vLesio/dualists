using UnityEngine;

public class GameState
{
    public bool IsRunning { get; private set; }
    public bool IsTimeout => IsRunning && (Time.time - StartTime >= GameDuration);
    public bool IsEnded => !IsRunning;
    public IPlayerController Winner { get; private set; }
    public bool IsTie => Winner == null && IsEnded;
    public float StartTime { get; set; }
    public float GameDuration { get; set; } = 60f; // Default game duration in seconds
    public float GameTimeProgressPercentage => (Time.time - StartTime)/GameDuration; 
    
    public void Start()
    {
        IsRunning = true;
        Winner = null;
        StartTime = Time.time;
    }

    public void End(IPlayerController winner = null)
    {
        IsRunning = false;
        Winner = winner;
        StartTime = 0;
    }
}