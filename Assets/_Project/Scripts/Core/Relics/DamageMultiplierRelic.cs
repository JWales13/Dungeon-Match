using System;

namespace Game.Core
{
    /// <summary>Increases all move damage by a percentage (applied to the running total).</summary>
    public class DamageMultiplierRelic : Relic
    {
        private readonly int _percentBonus;

        public override string DisplayName { get; }
        public override string Description => $"+{_percentBonus}% damage.";

        public DamageMultiplierRelic(string displayName, int percentBonus)
        {
            if (percentBonus <= 0) throw new ArgumentOutOfRangeException(nameof(percentBonus));
            DisplayName = displayName;
            _percentBonus = percentBonus;
        }

        public override int ModifyMoveDamage(int baseDamage, MoveOutcome move)
        {
            return baseDamage * (100 + _percentBonus) / 100;
        }
    }
}