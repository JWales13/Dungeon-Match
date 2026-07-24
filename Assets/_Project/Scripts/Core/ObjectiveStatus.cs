namespace Game.Core
{
    /// <summary>
    /// The lifecycle state of a board objective (an encounter's win/lose state).
    /// Kept as its own type so any objective - combat, score target, collect-X -
    /// reports outcome the same way.
    /// </summary>
    public enum ObjectiveStatus
    {
        InProgress,
        Won,
        Lost
    }
}