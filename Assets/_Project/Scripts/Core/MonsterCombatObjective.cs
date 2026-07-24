using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The MVP combat rule: the player drains a monster's HP by clearing tiles,
    /// with a limited number of moves. Win when HP hits zero; lose when moves
    /// run out first. Cascades naturally deal more damage because more tiles
    /// clear in a single move - free "combo" feel with no extra code.
    ///
    /// Pure C# and free of any Board reference, so it is unit tested directly
    /// (see MonsterCombatObjectiveTests) without driving a random board.
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

            MaxHealth = monsterHealth;
            CurrentHealth = monsterHealth;
            MoveLimit = moveLimit;
            _damagePerTile = damagePerTile;
        }

        public void RegisterResolvedMove(int tilesClearedThisMove)
        {
            if (Status != ObjectiveStatus.InProgress)
            {
                return;
            }

            ApplyDamage(tilesClearedThisMove * _damagePerTile);
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