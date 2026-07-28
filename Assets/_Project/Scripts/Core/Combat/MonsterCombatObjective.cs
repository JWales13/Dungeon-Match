using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The combat rule: drain a monster's HP by clearing tiles within a move
    /// limit. Win at 0 HP; lose when moves run out first. Pure C# and unit
    /// tested directly (MonsterCombatObjectiveTests).
    ///
    /// Note: relic/booster damage modifiers were removed in the Phase 0 cleanup
    /// (DungeonVision pivot). Crafted-booster effects will re-enter combat in a
    /// later phase via the new station/booster system, not the old RelicSet.
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

        public void RegisterResolvedMove(MoveOutcome move)
        {
            if (Status != ObjectiveStatus.InProgress)
            {
                return;
            }

            ApplyDamage(move.Total * _damagePerTile);
            ConsumeMove();
            EvaluateOutcome();
        }

        private void ApplyDamage(int amount)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void ConsumeMove()
        {
            MovesUsed++;
            MovesChanged?.Invoke(MovesRemaining, MoveLimit);
        }

        private void EvaluateOutcome()
        {
            if (CurrentHealth <= 0)
            {
                SetStatus(ObjectiveStatus.Won);
            }
            else if (MovesRemaining <= 0)
            {
                SetStatus(ObjectiveStatus.Lost);
            }
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