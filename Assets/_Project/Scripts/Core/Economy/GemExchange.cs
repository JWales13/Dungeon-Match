using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The three Sponsor Bucks (gem) sinks: skip a Producer's in-progress
    /// timer, top up an ingredient color, or buy Gold outright. Owns nothing
    /// but the pricing - it spends from the given Wallet and applies the
    /// effect directly to whatever domain object was passed in. Every method
    /// is a no-op (returns false, spends nothing) if there's nothing to buy,
    /// e.g. skipping a station that isn't producing.
    /// </summary>
    public class GemExchange
    {
        private readonly Wallet _wallet;
        private readonly GemPricing _pricing;

        public GemExchange(Wallet wallet, GemPricing pricing)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _pricing = pricing;
        }

        /// <summary>Flat gem cost of TryTopUpIngredients, for a view to show before the player commits.</summary>
        public int IngredientTopUpCost => _pricing.IngredientTopUpCost;

        /// <summary>Flat gem cost of TryBuyGold, for a view to show before the player commits.</summary>
        public int GoldPurchaseCost => _pricing.GoldPurchaseCost;

        /// <summary>
        /// Spends gems to instantly finish a station's in-progress production
        /// unit. Cost scales with the time actually skipped (rounded up,
        /// minimum 1 gem). Returns false without spending if the station
        /// isn't producing or the player can't afford it.
        /// </summary>
        public bool TrySkipProduction(ProducerStation station)
        {
            if (station == null) throw new ArgumentNullException(nameof(station));

            if (!station.IsProducing)
            {
                return false;
            }

            int cost = SkipCost(station.SecondsRemaining);
            if (!_wallet.TrySpend(CurrencyType.SponsorBucks, cost))
            {
                return false;
            }

            station.FastForward(station.SecondsRemaining);
            return true;
        }

        /// <summary>Gems this instant would cost to skip a station with this many seconds left.</summary>
        public int SkipCost(float secondsRemaining)
        {
            return Mathf.Max(1, Mathf.CeilToInt(secondsRemaining / _pricing.SkipProductionSecondsPerGem));
        }

        /// <summary>Spends gems for a flat top-up of one ingredient color.</summary>
        public bool TryTopUpIngredients(IngredientInventory inventory, TileType color)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (color == TileType.None) throw new ArgumentOutOfRangeException(nameof(color));

            if (!_wallet.TrySpend(CurrencyType.SponsorBucks, _pricing.IngredientTopUpCost))
            {
                return false;
            }

            inventory.Add(color, _pricing.IngredientTopUpAmount);
            return true;
        }

        /// <summary>Spends gems for a flat amount of Gold, deposited into the same wallet.</summary>
        public bool TryBuyGold()
        {
            if (!_wallet.TrySpend(CurrencyType.SponsorBucks, _pricing.GoldPurchaseCost))
            {
                return false;
            }

            _wallet.Add(CurrencyType.Gold, _pricing.GoldPurchaseAmount);
            return true;
        }
    }
}
