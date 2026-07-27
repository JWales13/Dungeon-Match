using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The MVP combat rule: drain a monster's HP by clearing tiles within a
    /// move limit. Win at 0 HP; lose when moves run out first. Relics (held in
    /// a run-scoped RelicSet) adjust the move limit at construction and the
    /// damage of each resolved move - combat math stays in one place while the
    /// modifiers stay open-ended.
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
        private readonly RelicSet _relics;

        public MonsterCombatObjective(int monsterHealth, int moveLimit, int damagePerTile = 1, RelicSet relics = null)
        {
            if (monsterHealth <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHealth));
            if (moveLimit <= 0) throw new ArgumentOutOfRangeException(nameof(moveLimit));
            if (damagePerTile <= 0) throw new ArgumentOutOfRangeException(nameof(damagePerTile));

            _relics = relics ?? new RelicSet();
            _damagePerTile = damagePerTile;

            MaxHealth = monsterHealth;
            CurrentHealth = monsterHealth;
            MoveLimit = Mathf.Max(1, _relics.ModifyMoveLimit(moveLimit));
        }

        public void RegisterResolvedMove(MoveOutcome move)
        {
            if (Status != ObjectiveStatus.InProgress)
            {
                return;
            }

            int baseDamage = move.Total * _damagePerTile;
            int finalDamage = _relics.ModifyMoveDamage(baseDamage, move);

            ApplyDamage(finalDamage);
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