using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Tier multiplier data: must never make a floor easier (all multipliers >= 1).</summary>
    public class TierMultipliersTests
    {
        [Test]
        public void None_IsIdentity()
        {
            TierMultipliers none = TierMultipliers.None;

            Assert.AreEqual(1f, none.MonsterHealthMultiplier);
            Assert.AreEqual(1f, none.GoldRewardMultiplier);
            Assert.AreEqual(1, none.IngredientMultiplier);
        }

        [Test]
        public void Constructor_AcceptsValuesAtOrAboveOne()
        {
            var multipliers = new TierMultipliers(1.5f, 2f, 3);

            Assert.AreEqual(1.5f, multipliers.MonsterHealthMultiplier);
            Assert.AreEqual(2f, multipliers.GoldRewardMultiplier);
            Assert.AreEqual(3, multipliers.IngredientMultiplier);
        }

        [Test]
        public void Constructor_RejectsAnyMultiplierBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TierMultipliers(0.9f, 2f, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TierMultipliers(1.5f, 0.5f, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TierMultipliers(1.5f, 2f, 0));
        }
    }
}
