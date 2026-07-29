using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Producer-station rules: ingredient + time gating, buffer cap, and
    /// collect. Time is fed in via Tick so every case is deterministic.
    /// </summary>
    public class ProducerStationTests
    {
        private static ProducerStation Bench(IngredientInventory ingredients, int cost = 2, float seconds = 5f, int capacity = 3)
        {
            return new ProducerStation(BoosterType.Dynamite, TileType.Red, cost, seconds, capacity, ingredients);
        }

        [Test]
        public void StartsProducing_AndSpendsIngredientsUpFront()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 10);
            var station = Bench(ingredients, cost: 2, seconds: 5f);

            station.Tick(2f); // starts + 2s of progress, not done yet

            Assert.IsTrue(station.IsProducing);
            Assert.AreEqual(0, station.BufferCount);
            Assert.AreEqual(8, ingredients.GetCount(TileType.Red)); // spent 2 on start
        }

        [Test]
        public void CompletesAUnit_AfterEnoughTime()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 10);
            var station = Bench(ingredients, cost: 2, seconds: 5f);

            station.Tick(2f);
            station.Tick(4f); // total 6s >= 5s -> one unit

            Assert.AreEqual(1, station.BufferCount);
            Assert.IsFalse(station.IsProducing);
        }

        [Test]
        public void WithoutIngredients_NeverProduces()
        {
            var ingredients = new IngredientInventory(); // empty
            var station = Bench(ingredients, cost: 2, seconds: 5f);

            station.Tick(100f);

            Assert.AreEqual(0, station.BufferCount);
            Assert.IsFalse(station.IsProducing);
        }

        [Test]
        public void StopsAtBufferCapacity_AndSpendsNoMoreIngredients()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 100);
            var station = Bench(ingredients, cost: 1, seconds: 1f, capacity: 2);

            station.Tick(1f); // unit 1
            station.Tick(1f); // unit 2 (buffer full)
            int spentAfterFull = ingredients.GetCount(TileType.Red);
            station.Tick(1f); // should do nothing - buffer full

            Assert.AreEqual(2, station.BufferCount);
            Assert.IsTrue(station.IsBufferFull);
            Assert.AreEqual(spentAfterFull, ingredients.GetCount(TileType.Red)); // no extra spend
        }

        [Test]
        public void Collect_EmptiesBuffer_AndReturnsCount()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 100);
            var station = Bench(ingredients, cost: 1, seconds: 1f, capacity: 2);

            station.Tick(1f);
            station.Tick(1f); // buffer = 2

            int collected = station.Collect();

            Assert.AreEqual(2, collected);
            Assert.AreEqual(0, station.BufferCount);
        }

        [Test]
        public void InitialBufferCount_SeedsBuffer_ForCarryingOverOnUpgrade()
        {
            var ingredients = new IngredientInventory();
            var station = new ProducerStation(BoosterType.Dynamite, TileType.Red,
                ingredientCost: 2, productionSeconds: 5f, bufferCapacity: 3, ingredients, initialBufferCount: 2);

            Assert.AreEqual(2, station.BufferCount);
            Assert.IsFalse(station.IsBufferFull);
        }

        [Test]
        public void InitialBufferCount_AboveCapacity_Throws()
        {
            var ingredients = new IngredientInventory();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ProducerStation(BoosterType.Dynamite, TileType.Red,
                    ingredientCost: 2, productionSeconds: 5f, bufferCapacity: 2, ingredients, initialBufferCount: 3));
        }

        [Test]
        public void FastForward_CompletesMultipleUnits_InOneCall()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 100);
            var station = Bench(ingredients, cost: 1, seconds: 1f, capacity: 10);

            station.FastForward(3.5f); // 3 whole units, 0.5s left over into a 4th

            Assert.AreEqual(3, station.BufferCount);
            Assert.IsTrue(station.IsProducing);
            Assert.AreEqual(0.5f, station.SecondsRemaining, 0.0001f);
        }

        [Test]
        public void FastForward_StopsAtBufferCapacity_LeftoverTimeUnused()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 1000);
            var station = Bench(ingredients, cost: 1, seconds: 1f, capacity: 2);

            station.FastForward(1000f); // way more time than the buffer could ever hold

            Assert.AreEqual(2, station.BufferCount);
            Assert.IsTrue(station.IsBufferFull);
            Assert.IsFalse(station.IsProducing); // stopped, not left mid-unit past the cap
            Assert.AreEqual(998, ingredients.GetCount(TileType.Red)); // exactly 2 units spent, no more
        }

        [Test]
        public void FastForward_StopsWhenOutOfIngredients_LeftoverTimeUnused()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 2); // enough for exactly one unit at cost 2, then dry
            var station = Bench(ingredients, cost: 2, seconds: 1f, capacity: 10);

            station.FastForward(100f);

            Assert.AreEqual(1, station.BufferCount);
            Assert.IsFalse(station.IsProducing);
            Assert.AreEqual(0, ingredients.GetCount(TileType.Red));
        }

        [Test]
        public void FastForward_RespectsExistingProgress_BeforeStartingFresh()
        {
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 100);
            var station = Bench(ingredients, cost: 1, seconds: 5f, capacity: 10);

            station.Tick(3f); // 3s into the first unit, not done
            station.FastForward(4f); // 3 + 4 = 7s -> completes (needed 5), 2s left into the next

            Assert.AreEqual(1, station.BufferCount);
            Assert.IsTrue(station.IsProducing);
            Assert.AreEqual(3f, station.SecondsRemaining, 0.0001f); // 5 - 2 remaining
        }
    }
}