using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Fixed-cadence tier scheduling: Regular by default, Main Event / Sweeps Week on their intervals.</summary>
    public class FloorTierScheduleTests
    {
        private static FloorTierSchedule Schedule() => new FloorTierSchedule(mainEventInterval: 5, sweepsWeekInterval: 10);

        [TestCase(1, FloorTier.Regular)]
        [TestCase(4, FloorTier.Regular)]
        [TestCase(5, FloorTier.MainEvent)]
        [TestCase(15, FloorTier.MainEvent)]
        [TestCase(10, FloorTier.SweepsWeek)]
        [TestCase(20, FloorTier.SweepsWeek)]
        public void TierFor_MatchesExpectedCadence(int depth, FloorTier expected)
        {
            Assert.AreEqual(expected, Schedule().TierFor(depth));
        }

        [Test]
        public void SweepsWeek_OverridesMainEvent_WhenBothApply()
        {
            // 10 is a multiple of both 5 (Main Event) and 10 (Sweeps Week).
            Assert.AreEqual(FloorTier.SweepsWeek, Schedule().TierFor(10));
        }

        [Test]
        public void TierFor_DepthBelowOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Schedule().TierFor(0));
        }

        [Test]
        public void Constructor_RejectsNonPositiveIntervals()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloorTierSchedule(0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloorTierSchedule(5, 0));
        }
    }
}
