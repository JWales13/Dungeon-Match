using System;

namespace Game.Core
{
    /// <summary>
    /// Immutable description of a single room's encounter parameters. Pure
    /// data - a Run is an ordered list of these, and the composition root turns
    /// the current one into a live MonsterCombatObjective. Keeping it a value
    /// type with no behavior means run generation and tuning stay trivial to
    /// test and reason about.
    /// </summary>
    public readonly struct RoomDefinition
    {
        public int MonsterHealth { get; }
        public int MoveLimit { get; }
        public int DamagePerTile { get; }

        public RoomDefinition(int monsterHealth, int moveLimit, int damagePerTile)
        {
            if (monsterHealth <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHealth));
            if (moveLimit <= 0) throw new ArgumentOutOfRangeException(nameof(moveLimit));
            if (damagePerTile <= 0) throw new ArgumentOutOfRangeException(nameof(damagePerTile));

            MonsterHealth = monsterHealth;
            MoveLimit = moveLimit;
            DamagePerTile = damagePerTile;
        }
    }
}