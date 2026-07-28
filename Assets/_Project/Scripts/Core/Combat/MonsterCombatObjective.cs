using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The combat rule: drain a monster's HP by clearing tiles within a move
    /// limit. Win at 0 HP; lose when moves run out first.
    ///
    /// Damage and moves are decoupled on purpose: RegisterClears applies damage
    /// from any board settle (a swap match, a power-tile detonation, OR a
    /// crafted booster), while SpendMove is called only for a real player move
    /// (a swap). So a booster deals damage without costing a move.
    ///
    /// Pure C# and unit tested directly (MonsterCombatObjectiveTests).
    /// </summary>
    public class MonsterCombatObjective : IBoardObjective
    {
        public int MaxHealth { get; }
        public int CurrentHealth { get; private set; }
        public int MoveLimit { get; }
        public int MovesUsed { get; private set; }
        public int MovesRemaining => MoveLimit - MovesUsed;
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.InProgress;

        public event Action<ObjectiveStatus> StatusChanged;
        public event Action<int, int> HealthChanged; // (current, max)
        public event Action<int, int> MovesChanged;   // (remaining, limit)

        private readonly int _damagePerTile;

        public MonsterCombatObjective(int monsterHealth, int moveLimit, int damagePerTile = 1)
        {
            if (monsterHealth <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHealth));
            if (moveLimit <= 0) throw new ArgumentOutOfRangeException(nameof(moveLimit));
            if (damagePerTile <= 0) throw new ArgumentOutOfRangeException(nameof(damagePerTile));

            _damagePerTile = damagePerTile;
            MaxHealth = monsterHealth;
            CurrentHealth = monsterHealth;
            MoveLimit = moveLimit;
        }

        /// <summary>Applies damage from cleared tiles. Does NOT consume a move.</summary>
        public void RegisterClears(MoveOutcome move)
        {
            if (Status != ObjectiveStatus.InProgress)
            {
                return;
            }

            ApplyDamage(move.Total * _damagePerTile);

            if (CurrentHealth <= 0)
            {
                SetStatus(ObjectiveStatus.Won);
            }
        }

        /// <summary>Consumes one move (a player swap). Lose if none remain and the monster lives.</summary>
        public void SpendMove()
        {
            if (Status != ObjectiveStatus.InProgress)
            {
                return;
            }

            MovesUsed++;
            MovesChanged?.Invoke(MovesRemaining, MoveLimit);

            if (MovesRemaining <= 0 && CurrentHealth > 0)
            {
                SetStatus(ObjectiveStatus.Lost);
            }
        }

        private void ApplyDamage(int amount)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void SetStatus(ObjectiveStatus status)
        {
            if (Status == status)
            {
                return;
            }

            Status = status;
            StatusChanged?.Invoke(status);
        }
    }
}