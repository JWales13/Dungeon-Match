using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Station catalog data: construction guards and level lookup.</summary>
    public class StationDefinitionTests
    {
        private static StationLevelConfig Level(int cost = 10) => new StationLevelConfig(
            ingredientCost: 2, productionSeconds: 5f, bufferCapacity: 2, cost: cost);

        [Test]
        public void GetLevel_ReturnsMatchingConfig_OneBased()
        {
            var level1 = Level(cost: 10);
            var level2 = Level(cost: 25);
            var definition = new StationDefinition(BoosterType.AcidVial, TileType.Green,
                new List<StationLevelConfig> { level1, level2 });

            Assert.AreEqual(10, definition.GetLevel(1).Cost);
            Assert.AreEqual(25, definition.GetLevel(2).Cost);
            Assert.AreEqual(2, definition.MaxLevel);
        }

        [Test]
        public void GetLevel_OutOfRange_Throws()
        {
            var definition = new StationDefinition(BoosterType.AcidVial, TileType.Green,
                new List<StationLevelConfig> { Level() });

            Assert.Throws<ArgumentOutOfRangeException>(() => definition.GetLevel(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => definition.GetLevel(2));
        }

        [Test]
        public void Constructor_RejectsNoneIngredientColor_AndEmptyLevels()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StationDefinition(BoosterType.AcidVial, TileType.None, new List<StationLevelConfig> { Level() }));

            Assert.Throws<ArgumentException>(() =>
                new StationDefinition(BoosterType.AcidVial, TileType.Green, new List<StationLevelConfig>()));
        }
    }
}
