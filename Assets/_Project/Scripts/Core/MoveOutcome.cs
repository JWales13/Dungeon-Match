using System;

namespace Game.Core
{
    /// <summary>
    /// An immutable summary of what a single resolved move cleared: the total
    /// tile count plus a per-color breakdown. Relics read this to award
    /// color-specific or size-specific bonuses. Built up by MoveOutcomeBuilder
    /// as cascades resolve.
    /// </summary>
    public readonly struct MoveOutcome
    {
        private static readonly int ColorCount = 4; // Red, Blue, Green, Yellow

        private readonly int[] _countsByType;
        public int Total { get; }

        internal MoveOutcome(int[] countsByType)
        {
            _countsByType = countsByType ?? new int[ColorCount];

            int total = 0;
            foreach (int count in _countsByType)
            {
                total += count;
            }

            Total = total;
        }

        public int CountOf(TileType type)
        {
            int index = (int)type;
            if (_countsByType == null || index < 0 || index >= _countsByType.Length)
            {
                return 0;
            }

            return _countsByType[index];
        }
    }
}