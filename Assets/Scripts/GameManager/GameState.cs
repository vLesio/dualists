public class GameState
{
    public bool IsRunning { get; private set; }
    public bool IsEnded => !IsRunning;
    public IPlayerController Winner { get; private set; }
    public bool IsTie => Winner == null && IsEnded;

    public void Start()
    {
        IsRunning = true;
        Winner = null;
    }

    public void End(IPlayerController winner = null)
    {
        IsRunning = false;
        Winner = winner;
    }
}