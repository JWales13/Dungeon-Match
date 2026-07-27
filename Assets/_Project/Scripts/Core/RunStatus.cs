namespace Game.Core
{
    /// <summary>
    /// The lifecycle state of a whole run (a sequence of rooms), distinct from
    /// a single room's ObjectiveStatus. A run is Won only after the final room
    /// is cleared, and Lost the moment any room is failed.
    /// </summary>
    public enum RunStatus
    {
        InProgress,
        Won,
        Lost
    }
}