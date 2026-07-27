using System;

namespace Game.Core
{
    /// <summary>Adds a flat damage bonus to every resolved move.</summary>
    public class FlatMoveDamageRelic : Relic
    {
        private readonly int _bonus;

        public override string DisplayName { get; }
        public override string Description => $"+{_bonus} damage per move.";

        public FlatMoveDamageRelic(string displayName, int bonus)
        {
            if (bonus <= 0) throw new ArgumentOutOfRangeException(nameof(bonus));
            DisplayName = displayName;
            _bonus = bonus;
        }

        public override int ModifyMoveDamage(int baseDamage, MoveOutcome move) => baseDamage + _bonus;
    }
}