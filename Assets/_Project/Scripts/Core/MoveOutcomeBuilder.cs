using System;

namespace Game.Core
{
    /// <summary>
    /// Accumulates cleared tiles across all cascade steps of a single move,
    /// then produces an immutable MoveOutcome. Reused across moves via Reset()
    /// so no per-move allocation churn.
    /// </summary>
    public class MoveOutcomeBuilder
    {
        private const int ColorCount = 4; // Red, Blue, Green, Yellow

        private readonly int[] _counts = new int[ColorCount];

        public void Add(TileType type)
        {
            int index = (int)type;
            if (index >= 0 && index < ColorCount)
            {
                _counts[index]++;
            }
        }

        public MoveOutcome Build()
        {
            return new MoveOutcome((int[])_counts.Clone());
        }

        public void Reset()
        {
            Array.Clear(_counts, 0, _counts.Length);
        }
    }
}