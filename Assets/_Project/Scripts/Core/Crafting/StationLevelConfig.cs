using System;

namespace Game.Core
{
    /// <summary>
    /// One upgrade level's production stats for a Producer station, plus the
    /// Prize Voucher cost to *reach* this level from the previous one. Level 1's
    /// Cost is the station's build cost (there is no separate "build cost"
    /// field - building is just reaching level 1).
    /// </summary>
    public readonly struct StationLevelConfig
    {
        public int IngredientCost { get; }
        public float ProductionSeconds { get; }
        public int BufferCapacity { get; }
        public int Cost { get; }

        public StationLevelConfig(int ingredientCost, float productionSeconds, int bufferCapacity, int cost)
        {
            if (ingredientCost <= 0) throw new ArgumentOutOfRangeException(nameof(ingredientCost));
            if (productionSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(productionSeconds));
            if (bufferCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacity));
            if (cost <= 0) throw new ArgumentOutOfRangeException(nameof(cost));

            IngredientCost = ingredientCost;
            ProductionSeconds = productionSeconds;
            BufferCapacity = bufferCapacity;
            Cost = cost;
        }
    }
}
