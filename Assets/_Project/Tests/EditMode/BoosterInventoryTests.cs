using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>The crafted-booster stash's counting and spend behavior.</summary>
    public class BoosterInventoryTests
    {
        [Test]
        public void Add_AccumulatesPerType()
        {
            var inventory = new BoosterInventory();

            inventory.Add(BoosterType.Dynamite, 2);
            inventory.Add(BoosterType.Dynamite, 1);

            Assert.AreEqual(3, inventory.GetCount(BoosterType.Dynamite));
        }

        [Test]
        public void TrySpend_DeductsWhenEnough_ElseUnchanged()
        {
            var inventory = new BoosterInventory();
            inventory.Add(BoosterType.Dynamite, 2);

            Assert.IsTrue(inventory.TrySpend(BoosterType.Dynamite, 1));
            Assert.AreEqual(1, inventory.GetCount(BoosterType.Dynamite));

            Assert.IsFalse(inventory.TrySpend(BoosterType.Dynamite, 5));
            Assert.AreEqual(1, inventory.GetCount(BoosterType.Dynamite));
        }
    }
}