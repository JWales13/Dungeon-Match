using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Combat rules in isolation. Damage (RegisterClears) and moves (SpendMove)
    /// are now separate, so a booster can deal damage without costing a move.
    /// </summary>
    public class MonsterCombatObjectiveTests
    {
        /// <summary>A settle that cleared <paramref name="tiles"/> tiles (all one color).</summary>
        private static MoveOutcome Move(int tiles)
        {
            var builder = new MoveOutcomeBuilder();
            for (int i = 0; i < tiles; i++)
            {
                builder.Add(TileType.Red);
            }

            return builder.Build();
        }

        [Test]
        public void RegisterClears_DealsDamage_WithoutSpendingMoves()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 10, moveLimit: 5, damagePerTile: 2);

            objective.RegisterClears(Move(3)); // 3 * 2 = 6 damage

            Assert.AreEqual(4, objective.CurrentHealth);
            Assert.AreEqual(5, objective.MovesRemaining, "Clears must not consume a move.");
            Assert.AreEqual(ObjectiveStatus.InProgress, objective.Status);
        }

        [Test]
        public void ClearingToZeroHealth_Wins()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 6, moveLimit: 5);

            objective.RegisterClears(Move(6));

            Assert.AreEqual(0, objective.CurrentHealth);
            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
        }

        [Test]
        public void SpendingAllMoves_WithMonsterAlive_Loses()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 100, moveLimit: 2);

            objective.SpendMove();
            Assert.AreEqual(ObjectiveStatus.InProgress, objective.Status);

            objective.SpendMove();
            Assert.AreEqual(ObjectiveStatus.Lost, objective.Status);
        }

        [Test]
        public void LethalClearOnLastMove_Wins_NotLoses()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 3, moveLimit: 1);

            objective.RegisterClears(Move(3)); // kills it
            objective.SpendMove();             // last move spent, but already won

            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
        }

        [Test]
        public void ActionsAfterResolution_AreIgnored()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 3, moveLimit: 5);

            objective.RegisterClears(Move(3)); // wins
            objective.RegisterClears(Move(3)); // no-op
            objective.SpendMove();             // no-op

            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
            Assert.AreEqual(0, objective.MovesUsed);
        }
    }
}