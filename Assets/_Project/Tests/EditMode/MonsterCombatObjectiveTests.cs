using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// The combat rules are verified in isolation - no Board, no randomness,
    /// no Play mode. Because MonsterCombatObjective takes tiles-cleared as a
    /// plain integer, every win/lose edge case is deterministic and fast.
    /// </summary>
    public class MonsterCombatObjectiveTests
    {
        [Test]
        public void RegisterResolvedMove_DealsDamagePerTile()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 10, moveLimit: 5, damagePerTile: 2);

            objective.RegisterResolvedMove(tilesClearedThisMove: 3); // 3 tiles * 2 = 6 damage

            Assert.AreEqual(4, objective.CurrentHealth);
            Assert.AreEqual(ObjectiveStatus.InProgress, objective.Status);
        }

        [Test]
        public void DepletingHealth_WinsTheEncounter()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 6, moveLimit: 5);

            objective.RegisterResolvedMove(tilesClearedThisMove: 6);

            Assert.AreEqual(0, objective.CurrentHealth);
            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
        }

        [Test]
        public void RunningOutOfMoves_LosesTheEncounter()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 100, moveLimit: 2, damagePerTile: 1);

            objective.RegisterResolvedMove(tilesClearedThisMove: 1);
            Assert.AreEqual(ObjectiveStatus.InProgress, objective.Status);

            objective.RegisterResolvedMove(tilesClearedThisMove: 1);
            Assert.AreEqual(ObjectiveStatus.Lost, objective.Status);
        }

        [Test]
        public void MovesAfterResolution_AreIgnored()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 3, moveLimit: 5);

            objective.RegisterResolvedMove(tilesClearedThisMove: 3); // wins here
            objective.RegisterResolvedMove(tilesClearedThisMove: 3); // must be a no-op

            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
            Assert.AreEqual(1, objective.MovesUsed);
            Assert.AreEqual(0, objective.CurrentHealth);
        }
    }
}