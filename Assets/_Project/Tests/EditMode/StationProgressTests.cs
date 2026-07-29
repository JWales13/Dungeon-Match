using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Station build/upgrade gating against a Wallet. Three levels: build (10),
    /// upgrade to 2 (25), upgrade to 3 (50).
    /// </summary>
    public class StationProgressTests
    {
        private static StationDefinition ThreeLevelStation()
        {
            return new StationDefinition(BoosterType.AcidVial, TileType.Green, new List<StationLevelConfig>
            {
                new StationLevelConfig(ingredientCost: 2, productionSeconds: 5f, bufferCapacity: 1, cost: 10),
                new StationLevelConfig(ingredientCost: 2, productionSeconds: 4f, bufferCapacity: 2, cost: 25),
                new StationLevelConfig(ingredientCost: 2, productionSeconds: 3f, bufferCapacity: 3, cost: 50),
            });
        }

        [Test]
        public void NotBuilt_HasNoCurrentConfig_AndNextCostIsBuildCost()
        {
            var wallet = new Wallet();
            var progress = new StationProgress(ThreeLevelStation(), wallet);

            Assert.IsFalse(progress.IsBuilt);
            Assert.IsNull(progress.CurrentConfig);
            Assert.AreEqual(10, progress.NextCost);
        }

        [Test]
        public void TryAdvance_WithoutEnoughVouchers_Fails_AndStaysUnbuilt()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.PrizeVoucher, 5);
            var progress = new StationProgress(ThreeLevelStation(), wallet);

            bool advanced = progress.TryAdvance();

            Assert.IsFalse(advanced);
            Assert.IsFalse(progress.IsBuilt);
            Assert.AreEqual(5, wallet.GetBalance(CurrencyType.PrizeVoucher));
        }

        [Test]
        public void TryAdvance_Builds_SpendsVouchers_AndSetsLevel1Config()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.PrizeVoucher, 10);
            var progress = new StationProgress(ThreeLevelStation(), wallet);

            bool built = progress.TryAdvance();

            Assert.IsTrue(built);
            Assert.IsTrue(progress.IsBuilt);
            Assert.AreEqual(1, progress.Level);
            Assert.AreEqual(0, wallet.GetBalance(CurrencyType.PrizeVoucher));
            Assert.AreEqual(1, progress.CurrentConfig.Value.BufferCapacity);
            Assert.AreEqual(25, progress.NextCost);
        }

        [Test]
        public void TryAdvance_UpgradesThroughLevels_ThenBlocksAtMax()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.PrizeVoucher, 10 + 25 + 50);
            var progress = new StationProgress(ThreeLevelStation(), wallet);

            Assert.IsTrue(progress.TryAdvance()); // build -> level 1
            Assert.IsTrue(progress.TryAdvance()); // -> level 2
            Assert.IsTrue(progress.TryAdvance()); // -> level 3 (max)

            Assert.AreEqual(3, progress.Level);
            Assert.IsTrue(progress.IsMaxLevel);
            Assert.AreEqual(0, progress.NextCost);
            Assert.AreEqual(0, wallet.GetBalance(CurrencyType.PrizeVoucher));

            Assert.IsFalse(progress.TryAdvance()); // already maxed
            Assert.AreEqual(3, progress.Level);
        }

        [Test]
        public void CanAffordNext_ReflectsWalletBalance()
        {
            var wallet = new Wallet();
            var progress = new StationProgress(ThreeLevelStation(), wallet);

            Assert.IsFalse(progress.CanAffordNext);

            wallet.Add(CurrencyType.PrizeVoucher, 10);
            Assert.IsTrue(progress.CanAffordNext);
        }

        [Test]
        public void InitialLevel_CanSeedAnAlreadyBuiltStation()
        {
            var wallet = new Wallet();
            var progress = new StationProgress(ThreeLevelStation(), wallet, initialLevel: 2);

            Assert.IsTrue(progress.IsBuilt);
            Assert.AreEqual(2, progress.Level);
            Assert.AreEqual(2, progress.CurrentConfig.Value.BufferCapacity);
        }

        [Test]
        public void Changed_FiresOnSuccessfulAdvance_NotOnFailedAdvance()
        {
            var wallet = new Wallet();
            var progress = new StationProgress(ThreeLevelStation(), wallet);
            int changedCount = 0;
            progress.Changed += () => changedCount++;

            progress.TryAdvance(); // fails, no vouchers
            Assert.AreEqual(0, changedCount);

            wallet.Add(CurrencyType.PrizeVoucher, 10);
            progress.TryAdvance(); // succeeds
            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void Constructor_RejectsInitialLevelOutOfRange()
        {
            var wallet = new Wallet();
            Assert.Throws<ArgumentOutOfRangeException>(() => new StationProgress(ThreeLevelStation(), wallet, initialLevel: 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StationProgress(ThreeLevelStation(), wallet, initialLevel: -1));
        }
    }
}
