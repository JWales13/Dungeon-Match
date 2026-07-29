namespace Game.Core
{
    /// <summary>
    /// The difficulty tier a floor falls on. Most floors are Regular; Main
    /// Event and Sweeps Week are periodic "boss beat" spikes (see
    /// FloorTierSchedule) that hit harder and pay out more.
    /// </summary>
    public enum FloorTier
    {
        Regular,
        MainEvent,
        SweepsWeek
    }
}
