using System.Collections.Generic;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>The ingredient stash's counting and event behavior, in isolation.</summary>
    public class IngredientInventoryTests
    {
        [Test]
        public void Add_AccumulatesPerColor()
        {
            var inventory = new IngredientInventory();

            inventory.Add(TileType.Red, 2);
            inventory.Add(TileType.Red, 3);
            inventory.Add(TileType.Blue, 1);

            Assert.AreEqual(5, inventory.GetCount(TileType.Red));
            Assert.AreEqual(1, inventory.GetCount(TileType.Blue));
            Assert.AreEqual(0, inventory.GetCount(TileType.Green));
        }

        [Test]
        public void Add_IgnoresNonPositiveAndNone()
        {
            var inventory = new IngredientInventory();

            inventory.Add(TileType.Red, 0);
            inventory.Add(TileType.Red, -5);
            inventory.Add(TileType.None, 3);

            Assert.AreEqual(0, inventory.GetCount(TileType.Red));
        }

        [Test]
        public void Add_RaisesChanged()
        {
            var inventory = new IngredientInventory();
            int changes = 0;
            inventory.Changed += () => changes++;

            inventory.Add(TileType.Green, 1);

            Assert.AreEqual(1, changes);
        }

        [Test]
        public void Constructor_SeedsFromInitialCounts()
        {
            var initial = new Dictionary<TileType, int> { { TileType.Yellow, 4 } };

            var inventory = new IngredientInventory(initial);

            Assert.AreEqual(4, inventory.GetCount(TileType.Yellow));
        }

        [Test]
        public void TrySpend_DeductsWhenEnough_ElseUnchanged()
        {
            var inventory = new IngredientInventory();
            inventory.Add(TileType.Red, 5);

            Assert.IsTrue(inventory.TrySpend(TileType.Red, 3));
            Assert.AreEqual(2, inventory.GetCount(TileType.Red));

            Assert.IsFalse(inventory.TrySpend(TileType.Red, 5));
            Assert.AreEqual(2, inventory.GetCount(TileType.Red));
        }
    }
}