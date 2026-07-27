using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Relic effects, stacking, and reward selection - all pure, all
    /// deterministic. Each relic is verified against its hook directly.
    /// </summary>
    public class RelicTests
    {
        private static MoveOutcome MoveOf(TileType type, int count)
        {
            var builder = new MoveOutcomeBuilder();
            for (int i = 0; i < count; i++)
            {
                builder.Add(type);
            }

            return builder.Build();
        }

        [Test]
        public void ExtraStartingMoves_AddsToMoveLimit()
        {
            var relic = new ExtraStartingMovesRelic("Adrenaline", extraMoves: 3);

            Assert.AreEqual(13, relic.ModifyMoveLimit(10));
        }

        [Test]
        public void FlatMoveDamage_AddsFlatBonus()
        {
            var relic = new FlatMoveDamageRelic("Brass Knuckles", bonus: 2);

            Assert.AreEqual(7, relic.ModifyMoveDamage(5, MoveOf(TileType.Blue, 3)));
        }

        [Test]
        public void ColorTileBonus_ScalesWithMatchingColorOnly()
        {
            var relic = new ColorTileBonusRelic("Bloodstone", TileType.Red, bonusPerTile: 2);

            // 3 red tiles -> +6; blue tiles are ignored.
            Assert.AreEqual(6, relic.ModifyMoveDamage(0, MoveOf(TileType.Red, 3)));
            Assert.AreEqual(0, relic.ModifyMoveDamage(0, MoveOf(TileType.Blue, 3)));
        }

        [Test]
        public void BigMatchBonus_OnlyAppliesAtThreshold()
        {
            var relic = new BigMatchBonusRelic("Avalanche", threshold: 6, bonus: 5);

            Assert.AreEqual(5, relic.ModifyMoveDamage(0, MoveOf(TileType.Green, 6)));
            Assert.AreEqual(0, relic.ModifyMoveDamage(0, MoveOf(TileType.Green, 5)));
        }

        [Test]
        public void DamageMultiplier_ScalesTheRunningTotal()
        {
            var relic = new DamageMultiplierRelic("Overclock", percentBonus: 25);

            Assert.AreEqual(125, relic.ModifyMoveDamage(100, MoveOf(TileType.Yellow, 1)));
        }

        [Test]
        public void RelicSet_AppliesAllRelicsInOrder()
        {
            var set = new RelicSet();
            set.Add(new FlatMoveDamageRelic("Brass Knuckles", bonus: 2));   // +2 first
            set.Add(new DamageMultiplierRelic("Overclock", percentBonus: 50)); // then x1.5

            // base 4 -> +2 = 6 -> x1.5 = 9
            Assert.AreEqual(9, set.ModifyMoveDamage(4, MoveOf(TileType.Red, 4)));
        }

        [Test]
        public void RewardGenerator_ReturnsRequestedDistinctCount()
        {
            var pool = RelicCatalog.CreateDefault();

            var options = RelicRewardGenerator.PickOptions(pool, optionCount: 2, random: new Random(1));

            Assert.AreEqual(2, options.Count);
            Assert.AreNotSame(options[0], options[1]);
        }

        [Test]
        public void RewardGenerator_NeverExceedsPoolSize()
        {
            var pool = RelicCatalog.CreateDefault();

            var options = RelicRewardGenerator.PickOptions(pool, optionCount: pool.Count + 5, random: new Random(2));

            Assert.AreEqual(pool.Count, options.Count);
        }
    }
}