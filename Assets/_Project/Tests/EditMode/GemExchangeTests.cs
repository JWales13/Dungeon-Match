using System;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// The three Sponsor Bucks spends: skip a Producer's timer, top up an
    /// ingredient color, buy Gold. Each should be a no-op (nothing spent, no
    /// effect) when there's nothing to buy or the player can't afford it.
    /// </summary>
    public class GemExchangeTests
    {
        private static GemPricing Pricing()
        {
            // 1 gem per 10s remaining, 5 gems for +20 ingredients, 10 gems for +100 gold
            return new GemPricing(skipProductionSecondsPerGem: 10f, ingredientTopUpCost: 5,
                ingredientTopUpAmount: 20, goldPurchaseCost: 10, goldPurchaseAmount: 100);
        }

        private static ProducerStation Bench(IngredientInventory ingredients, int cost = 2, float seconds = 20f, int capacity = 3)
        {
            return new ProducerStation(BoosterType.Dynamite, TileType.Red, cost, seconds, capacity, ingredients);
        }

        [Test]
        public void SkipProduction_FinishesTheUnit_AndSpendsGems()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 10);
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 10);
            var station = Bench(ingredients, cost: 2, seconds: 20f);
            station.Tick(5f); // 15s remaining -> ceil(15/10) = 2 gems
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TrySkipProduction(station);

            Assert.IsTrue(result);
            Assert.AreEqual(1, station.BufferCount);
            Assert.IsFalse(station.IsProducing);
            Assert.AreEqual(8, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void SkipProduction_WhenNotProducing_DoesNothing()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 10);
            var ingredients = new IngredientInventory(); // no ingredients -> never starts
            var station = Bench(ingredients);
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TrySkipProduction(station);

            Assert.IsFalse(result);
            Assert.AreEqual(10, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void SkipProduction_WithoutEnoughGems_DoesNothing()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 1);
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Red, 10);
            var station = Bench(ingredients, cost: 2, seconds: 20f);
            station.Tick(1f); // 19s remaining -> ceil(19/10) = 2 gems, only have 1
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TrySkipProduction(station);

            Assert.IsFalse(result);
            Assert.AreEqual(0, station.BufferCount);
            Assert.AreEqual(1, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void SkipCost_RoundsUp_WithMinimumOfOne()
        {
            var exchange = new GemExchange(new Wallet(), Pricing());

            Assert.AreEqual(1, exchange.SkipCost(0.1f)); // rounds up from ~0
            Assert.AreEqual(2, exchange.SkipCost(15f));  // 15/10 = 1.5 -> 2
            Assert.AreEqual(1, exchange.SkipCost(10f));  // exact
        }

        [Test]
        public void TopUpIngredients_AddsAmount_AndSpendsGems()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 5);
            var ingredients = new IngredientInventory();
            ingredients.Add(TileType.Green, 3);
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TryTopUpIngredients(ingredients, TileType.Green);

            Assert.IsTrue(result);
            Assert.AreEqual(23, ingredients.GetCount(TileType.Green));
            Assert.AreEqual(0, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void TopUpIngredients_WithoutEnoughGems_DoesNothing()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 4);
            var ingredients = new IngredientInventory();
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TryTopUpIngredients(ingredients, TileType.Green);

            Assert.IsFalse(result);
            Assert.AreEqual(0, ingredients.GetCount(TileType.Green));
            Assert.AreEqual(4, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void BuyGold_AddsGold_AndSpendsGems()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 10);
            wallet.Add(CurrencyType.Gold, 50);
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TryBuyGold();

            Assert.IsTrue(result);
            Assert.AreEqual(150, wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(0, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void BuyGold_WithoutEnoughGems_DoesNothing()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.SponsorBucks, 9);
            var exchange = new GemExchange(wallet, Pricing());

            bool result = exchange.TryBuyGold();

            Assert.IsFalse(result);
            Assert.AreEqual(0, wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(9, wallet.GetBalance(CurrencyType.SponsorBucks));
        }

        [Test]
        public void Constructor_NullWallet_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GemExchange(null, Pricing()));
        }
    }
}
