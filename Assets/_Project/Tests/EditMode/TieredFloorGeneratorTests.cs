using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Composing the base curve with tier scheduling/multipliers into the final FloorSpec.</summary>
    public class TieredFloorGeneratorTests
    {
        private static FloorDifficultyCurve Curve() => new FloorDifficultyCurve(
            baseMonsterHealth: 30, monsterHealthPerDepth: 6,
            baseMoveLimit: 15, moveLimitPerDepth: 0, maxMoveLimit: 15,
            baseBoardSize: 8, boardSizePerDepth: 0, maxBoardSize: 8,
            baseGoldReward: 25, goldRewardPerDepth: 5);

        private static TieredFloorGenerator Generator()
        {
            return new TieredFloorGenerator(
                Curve(),
                new FloorTierSchedule(mainEventInterval: 5, sweepsWeekInterval: 10),
                mainEventMultipliers: new TierMultipliers(1.5f, 1.5f, 2),
                sweepsWeekMultipliers: new TierMultipliers(2f, 2.5f, 3));
        }

        [Test]
        public void RegularFloor_PassesBaseCurveThroughUnchanged()
        {
            FloorSpec spec = Generator().Generate(3); // not a multiple of 5 or 10

            Assert.AreEqual(FloorTier.Regular, spec.Tier);
            Assert.AreEqual(1, spec.IngredientMultiplier);
            Assert.AreEqual(30 + 6 * 2, spec.MonsterHealth); // depthIndex 2, unmultiplied
            Assert.AreEqual(25 + 5 * 2, spec.GoldReward);
        }

        [Test]
        public void MainEventFloor_ScalesHealthAndGold_AndSetsIngredientMultiplier()
        {
            FloorSpec spec = Generator().Generate(5); // depthIndex 4, Main Event

            int baseHealth = 30 + 6 * 4;
            int baseGold = 25 + 5 * 4;

            Assert.AreEqual(FloorTier.MainEvent, spec.Tier);
            Assert.AreEqual(2, spec.IngredientMultiplier);
            Assert.AreEqual((int)System.Math.Round(baseHealth * 1.5f), spec.MonsterHealth);
            Assert.AreEqual((int)System.Math.Round(baseGold * 1.5f), spec.GoldReward);
        }

        [Test]
        public void SweepsWeekFloor_ScalesHarderThanMainEvent()
        {
            FloorSpec spec = Generator().Generate(10); // depthIndex 9, Sweeps Week (multiple of both)

            int baseHealth = 30 + 6 * 9;
            int baseGold = 25 + 5 * 9;

            Assert.AreEqual(FloorTier.SweepsWeek, spec.Tier);
            Assert.AreEqual(3, spec.IngredientMultiplier);
            Assert.AreEqual((int)System.Math.Round(baseHealth * 2f), spec.MonsterHealth);
            Assert.AreEqual((int)System.Math.Round(baseGold * 2.5f), spec.GoldReward);
        }

        [Test]
        public void BoardSizeAndMoveLimit_AreNeverTierMultiplied()
        {
            FloorSpec regular = Generator().Generate(3);
            FloorSpec sweepsWeek = Generator().Generate(10);

            Assert.AreEqual(regular.BoardSize, sweepsWeek.BoardSize);
            Assert.AreEqual(regular.MoveLimit, sweepsWeek.MoveLimit);
        }

        [Test]
        public void Constructor_RejectsNullCurveOrSchedule()
        {
            Assert.Throws<ArgumentNullException>(() => new TieredFloorGenerator(
                null, new FloorTierSchedule(5, 10), TierMultipliers.None, TierMultipliers.None));

            Assert.Throws<ArgumentNullException>(() => new TieredFloorGenerator(
                Curve(), null, TierMultipliers.None, TierMultipliers.None));
        }
    }
}
