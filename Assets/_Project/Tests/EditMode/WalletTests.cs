using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>Currency balances: add and spend.</summary>
    public class WalletTests
    {
        [Test]
        public void Add_AccumulatesPerCurrency()
        {
            var wallet = new Wallet();

            wallet.Add(CurrencyType.Gold, 50);
            wallet.Add(CurrencyType.Gold, 25);
            wallet.Add(CurrencyType.PrizeVoucher, 1);

            Assert.AreEqual(75, wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(1, wallet.GetBalance(CurrencyType.PrizeVoucher));
        }

        [Test]
        public void TrySpend_DeductsWhenEnough_ElseUnchanged()
        {
            var wallet = new Wallet();
            wallet.Add(CurrencyType.Gold, 30);

            Assert.IsTrue(wallet.TrySpend(CurrencyType.Gold, 20));
            Assert.AreEqual(10, wallet.GetBalance(CurrencyType.Gold));

            Assert.IsFalse(wallet.TrySpend(CurrencyType.Gold, 50));
            Assert.AreEqual(10, wallet.GetBalance(CurrencyType.Gold));
        }
    }
}