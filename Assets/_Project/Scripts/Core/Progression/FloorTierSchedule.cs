using System;

namespace Game.Core
{
    /// <summary>
    /// Which tier a depth falls on, on a fixed cadence: every
    /// mainEventInterval-th floor is a Main Event, every
    /// sweepsWeekInterval-th is a Sweeps Week. If a depth is a multiple of
    /// both, Sweeps Week wins (it's the bigger spike). Everything else is
    /// Regular.
    /// </summary>
    public class FloorTierSchedule
    {
        private readonly int _mainEventInterval;
        private readonly int _sweepsWeekInterval;

        public FloorTierSchedule(int mainEventInterval, int sweepsWeekInterval)
        {
            if (mainEventInterval <= 0) throw new ArgumentOutOfRangeException(nameof(mainEventInterval));
            if (sweepsWeekInterval <= 0) throw new ArgumentOutOfRangeException(nameof(sweepsWeekInterval));

            _mainEventInterval = mainEventInterval;
            _sweepsWeekInterval = sweepsWeekInterval;
        }

        public FloorTier TierFor(int depth)
        {
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));

            if (depth % _sweepsWeekInterval == 0)
            {
                return FloorTier.SweepsWeek;
            }

            if (depth % _mainEventInterval == 0)
            {
                return FloorTier.MainEvent;
            }

            return FloorTier.Regular;
        }
    }
}
