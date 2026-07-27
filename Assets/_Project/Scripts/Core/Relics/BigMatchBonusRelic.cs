using System;

namespace Game.Core
{
    /// <summary>Awards bonus damage when a move clears at least a threshold of tiles.</summary>
    public class BigMatchBonusRelic : Relic
    {
        private readonly int _threshold;
        private readonly int _bonus;

        public override string DisplayName { get; }
        public override string Description => $"Clear {_threshold}+ tiles in a move: +{_bonus} damage.";

        public BigMatchBonusRelic(string displayName, int threshold, int bonus)
        {
            if (threshold <= 0) throw new ArgumentOutOfRangeException(nameof(threshold));
            if (bonus <= 0) throw new ArgumentOutOfRangeException(nameof(bonus));
            DisplayName = displayName;
            _threshold = threshold;
            _bonus = bonus;
        }

        public override int ModifyMoveDamage(int baseDamage, MoveOutcome move)
        {
            return move.Total >= _threshold ? baseDamage + _bonus : baseDamage;
        }
    }
}