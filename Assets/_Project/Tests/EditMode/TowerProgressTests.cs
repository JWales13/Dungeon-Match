using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Depth tracking: starts at 1, only ever advances, fires Changed.</summary>
    public class TowerProgressTests
    {
        [Test]
        public void DefaultConstructor_StartsAtDepth1()
        {
            var progress = new TowerProgress();

            Assert.AreEqual(1, progress.CurrentDepth);
        }

        [Test]
        public void Constructor_AcceptsAnExplicitInitialDepth()
        {
            var progress = new TowerProgress(initialDepth: 12);

            Assert.AreEqual(12, progress.CurrentDepth);
        }

        [Test]
        public void Constructor_RejectsDepthBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TowerProgress(initialDepth: 0));
        }

        [Test]
        public void AdvanceDepth_IncrementsByOne_AndFiresChanged()
        {
            var progress = new TowerProgress();
            int changedCount = 0;
            progress.Changed += () => changedCount++;

            progress.AdvanceDepth();
            progress.AdvanceDepth();

            Assert.AreEqual(3, progress.CurrentDepth);
            Assert.AreEqual(2, changedCount);
        }
    }
}

