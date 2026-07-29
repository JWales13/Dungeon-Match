using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Depth -> crate count: none before the starting depth, then a growing, capped percentage of the board.</summary>
    public class CrateScheduleTests
    {
        private static CrateSchedule Schedule() => new CrateSchedule(
            startingDepth: 3, basePercentage: 0.10f, percentagePerDepth: 0.02f, maxPercentage: 0.25f);

        [Test]
        public void BeforeStartingDepth_NoCrates()
        {
            Assert.AreEqual(0, Schedule().CrateCountFor(1, 8, 8));
            Assert.AreEqual(0, Schedule().CrateCountFor(2, 8, 8));
        }

        [Test]
        public void AtStartingDepth_UsesBasePercentage()
        {
            // 8x8 = 64 cells, 10% = 6.4 -> rounds to 6.
            Assert.AreEqual(6, Schedule().CrateCountFor(3, 8, 8));
        }

        [Test]
        public void PastStartingDepth_GrowsLinearly()
        {
            // Depth 8 = 5 depths past starting (3): 10% + 5*2% = 20% of 64 = 12.8 -> 13.
            Assert.AreEqual(13, Schedule().CrateCountFor(8, 8, 8));
        }

        [Test]
        public void FarPastStartingDepth_CapsAtMaxPercentage()
        {
            // Way past the point where 10% + n*2% would exceed 25%.
            int atCap = Schedule().CrateCountFor(3 + 50, 8, 8);

            Assert.AreEqual((int)Math.Round(64 * 0.25f), atCap);
        }

        [Test]
        public void Constructor_RejectsInvalidPercentagesAndDepth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrateSchedule(0, 0.1f, 0.02f, 0.25f)); // depth < 1
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrateSchedule(3, -0.1f, 0.02f, 0.25f)); // negative base
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrateSchedule(3, 0.1f, -0.02f, 0.25f)); // negative growth
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrateSchedule(3, 0.3f, 0.02f, 0.25f));  // max below base
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrateSchedule(3, 0.1f, 0.02f, 1.5f));   // max above 100%
        }

        [Test]
        public void CrateCountFor_RejectsNonPositiveDimensionsOrDepth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Schedule().CrateCountFor(0, 8, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Schedule().CrateCountFor(3, 0, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Schedule().CrateCountFor(3, 8, 0));
        }
    }
}
