using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Every field of GemPricing must be a usable positive value.</summary>
    public class GemPricingTests
    {
        [Test]
        public void ZeroOrNegativeSkipSecondsPerGem_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GemPricing(0f, 5, 20, 10, 100));
        }

        [Test]
        public void ZeroOrNegativeTopUpCostOrAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GemPricing(10f, 0, 20, 10, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GemPricing(10f, 5, 0, 10, 100));
        }

        [Test]
        public void ZeroOrNegativeGoldCostOrAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GemPricing(10f, 5, 20, 0, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GemPricing(10f, 5, 20, 10, 0));
        }

        [Test]
        public void ValidValues_ConstructWithoutThrowing()
        {
            Assert.DoesNotThrow(() => new GemPricing(10f, 5, 20, 10, 100));
        }
    }
}
