using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Depth -> FloorSpec generation: linear growth, caps, and validation.</summary>
    public class FloorDifficultyCurveTests
    {
        private static FloorDifficultyCurve Curve()
        {
            return new FloorDifficultyCurve(
                baseMonsterHealth: 30, monsterHealthPerDepth: 6,
                baseMoveLimit: 15, moveLimitPerDepth: 1, maxMoveLimit: 18,
                baseBoardSize: 8, boardSizePerDepth: 1, maxBoardSize: 20,
                baseGoldReward: 25, goldRewardPerDepth: 5);
        }

        [Test]
        public void Depth1_ReturnsExactlyBaseValues()
        {
            FloorSpec spec = Curve().Generate(1);

            Assert.AreEqual(1, spec.Depth);
            Assert.AreEqual(30, spec.MonsterHealth);
            Assert.AreEqual(15, spec.MoveLimit);
            Assert.AreEqual(8, spec.BoardSize);
            Assert.AreEqual(25, spec.GoldReward);
        }

        [Test]
        public void DeeperFloors_GrowLinearly()
        {
            FloorSpec spec = Curve().Generate(4); // depthIndex = 3

            Assert.AreEqual(30 + 6 * 3, spec.MonsterHealth);
            Assert.AreEqual(15 + 1 * 3, spec.MoveLimit);
            Assert.AreEqual(8 + 1 * 3, spec.BoardSize);
            Assert.AreEqual(25 + 5 * 3, spec.GoldReward);
        }

        [Test]
        public void MoveLimitAndBoardSize_CapAtMax_ButHealthAndGoldKeepClimbing()
        {
            FloorSpec spec = Curve().Generate(50); // way past both caps

            Assert.AreEqual(18, spec.MoveLimit); // capped
            Assert.AreEqual(20, spec.BoardSize);  // capped
            Assert.AreEqual(30 + 6 * 49, spec.MonsterHealth); // uncapped
            Assert.AreEqual(25 + 5 * 49, spec.GoldReward);    // uncapped
        }

        [Test]
        public void Generate_DepthBelowOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Curve().Generate(0));
        }

        [Test]
        public void Constructor_RejectsInvertedCaps()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloorDifficultyCurve(
                baseMonsterHealth: 30, monsterHealthPerDepth: 6,
                baseMoveLimit: 15, moveLimitPerDepth: 1, maxMoveLimit: 10, // below base
                baseBoardSize: 8, boardSizePerDepth: 1, maxBoardSize: 10,
                baseGoldReward: 25, goldRewardPerDepth: 5));

            Assert.Throws<ArgumentOutOfRangeException>(() => new FloorDifficultyCurve(
                baseMonsterHealth: 30, monsterHealthPerDepth: 6,
                baseMoveLimit: 15, moveLimitPerDepth: 1, maxMoveLimit: 18,
                baseBoardSize: 8, boardSizePerDepth: 1, maxBoardSize: 5, // below base
                baseGoldReward: 25, goldRewardPerDepth: 5));
        }

        [Test]
        public void Constructor_RejectsNegativeGrowth_AndNonPositiveBases()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloorDifficultyCurve(
                baseMonsterHealth: 30, monsterHealthPerDepth: -1,
                baseMoveLimit: 15, moveLimitPerDepth: 1, maxMoveLimit: 18,
                baseBoardSize: 8, boardSizePerDepth: 1, maxBoardSize: 10,
                baseGoldReward: 25, goldRewardPerDepth: 5));

            Assert.Throws<ArgumentOutOfRangeException>(() => new FloorDifficultyCurve(
                baseMonsterHealth: 0, monsterHealthPerDepth: 6,
                baseMoveLimit: 15, moveLimitPerDepth: 1, maxMoveLimit: 18,
                baseBoardSize: 8, boardSizePerDepth: 1, maxBoardSize: 10,
                baseGoldReward: 25, goldRewardPerDepth: 5));
        }
    }
}
