using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Combat rules verified in isolation - no Board, no randomness, no Play
    /// mode. Moves are built as MoveOutcomes of a single color; combat that
    /// ignores color simply reads Total.
    /// </summary>
    public class MonsterCombatObjectiveTests
    {
        /// <summary>A move that cleared <paramref name="tiles"/> tiles (all one color).</summary>
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
        public void RegisterResolvedMove_DealsDamagePerTile()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 10, moveLimit: 5, damagePerTile: 2);

            objective.RegisterResolvedMove(Move(3)); // 3 tiles * 2 = 6 damage

            Assert.AreEqual(4, objective.CurrentHealth);
            Assert.AreEqual(ObjectiveStatus.InProgress, objective.Status);
        }

        [Test]
        public void DepletingHealth_WinsTheEncounter()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 6, moveLimit: 5);

            objective.RegisterResolvedMove(Move(6));

            Assert.AreEqual(0, objective.CurrentHealth);
            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
        }

        [Test]
        public void RunningOutOfMoves_LosesTheEncounter()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 100, moveLimit: 2, damagePerTile: 1);

            objective.RegisterResolvedMove(Move(1));
            Assert.AreEqual(ObjectiveStatus.InProgress, objective.Status);

            objective.RegisterResolvedMove(Move(1));
            Assert.AreEqual(ObjectiveStatus.Lost, objective.Status);
        }

        [Test]
        public void MovesAfterResolution_AreIgnored()
        {
            var objective = new MonsterCombatObjective(monsterHealth: 3, moveLimit: 5);

            objective.RegisterResolvedMove(Move(3)); // wins here
            objective.RegisterResolvedMove(Move(3)); // must be a no-op

            Assert.AreEqual(ObjectiveStatus.Won, objective.Status);
            Assert.AreEqual(1, objective.MovesUsed);
            Assert.AreEqual(0, objective.CurrentHealth);
        }

        [Test]
        public void Relics_IncreaseMoveLimitAndDamage()
        {
            var relics = new RelicSet();
            relics.Add(new ExtraStartingMovesRelic("Adrenaline", extraMoves: 2));
            relics.Add(new FlatMoveDamageRelic("Brass Knuckles", bonus: 3));

            var objective = new MonsterCombatObjective(monsterHealth: 20, moveLimit: 5, damagePerTile: 1, relics: relics);

            Assert.AreEqual(7, objective.MoveLimit); // 5 + 2

            objective.RegisterResolvedMove(Move(4)); // (4 * 1) + 3 = 7 damage
            Assert.AreEqual(13, objective.CurrentHealth);
        }
    }
}