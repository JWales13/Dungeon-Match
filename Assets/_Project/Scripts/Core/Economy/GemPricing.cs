using System;

namespace Game.Core
{
    /// <summary>
    /// Tunable costs/amounts for every Sponsor Bucks (gem) spend. A plain data
    /// struct so a Gameplay ScriptableObject can hold the tuned numbers while
    /// Core only knows the shape. Skipping a Producer's timer scales with the
    /// time actually skipped (rounded up, minimum 1 gem); the other two are
    /// flat prices.
    /// </summary>
    public readonly struct GemPricing
    {
        public readonly float SkipProductionSecondsPerGem;
        public readonly int IngredientTopUpCost;
        public readonly int IngredientTopUpAmount;
        public readonly int GoldPurchaseCost;
        public readonly int GoldPurchaseAmount;

        public GemPricing(float skipProductionSecondsPerGem, int ingredientTopUpCost, int ingredientTopUpAmount,
            int goldPurchaseCost, int goldPurchaseAmount)
        {
            if (skipProductionSecondsPerGem <= 0f) throw new ArgumentOutOfRangeException(nameof(skipProductionSecondsPerGem));
            if (ingredientTopUpCost <= 0) throw new ArgumentOutOfRangeException(nameof(ingredientTopUpCost));
            if (ingredientTopUpAmount <= 0) throw new ArgumentOutOfRangeException(nameof(ingredientTopUpAmount));
            if (goldPurchaseCost <= 0) throw new ArgumentOutOfRangeException(nameof(goldPurchaseCost));
            if (goldPurchaseAmount <= 0) throw new ArgumentOutOfRangeException(nameof(goldPurchaseAmount));

            SkipProductionSecondsPerGem = skipProductionSecondsPerGem;
            IngredientTopUpCost = ingredientTopUpCost;
            IngredientTopUpAmount = ingredientTopUpAmount;
            GoldPurchaseCost = goldPurchaseCost;
            GoldPurchaseAmount = goldPurchaseAmount;
        }
    }
}
