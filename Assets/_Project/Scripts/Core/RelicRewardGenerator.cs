using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Picks a set of distinct relic options to offer as a post-room reward.
    /// Takes an explicit Random so results are deterministic (and testable)
    /// when seeded. Returns fewer than requested only if the pool is smaller.
    /// </summary>
    public static class RelicRewardGenerator
    {
        public static IReadOnlyList<IRelic> PickOptions(IReadOnlyList<IRelic> pool, int optionCount, Random random)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var remaining = new List<IRelic>(pool);
            var chosen = new List<IRelic>();

            int drawCount = Math.Min(Math.Max(optionCount, 0), remaining.Count);
            for (int i = 0; i < drawCount; i++)
            {
                int index = random.Next(remaining.Count);
                chosen.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            return chosen;
        }
    }
}