
public enum GamePhase
{
    NotStarted,
    Running,
    EndedWithWin,
    EndedWithTie
}

public readonly struct GameState
{
    public GamePhase Phase { get; }
    public string Winner { get; }

    private GameState(GamePhase phase, string winner = null)
    {
        Phase = phase;
        Winner = winner;
    }

    public static GameState NotStarted => new GameState(GamePhase.NotStarted);
    public static GameState Running => new GameState(GamePhase.Running);
    public static GameState EndedWithWinner(string winner) => new GameState(GamePhase.EndedWithWin, winner);
    public static GameState EndedWithTie => new GameState(GamePhase.EndedWithTie);

    public bool IsRunning => Phase == GamePhase.Running;
    public bool IsEnded => Phase == GamePhase.EndedWithWin || Phase == GamePhase.EndedWithTie;
    public bool IsTie => Phase == GamePhase.EndedWithTie;
    public bool HasWinner => Phase == GamePhase.EndedWithWin && !string.IsNullOrEmpty(Winner);
}