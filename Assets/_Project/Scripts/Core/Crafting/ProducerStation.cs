using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// A crafting station that turns one ingredient plus real time into one
    /// booster, held in a small buffer the player collects. Auto-produces while
    /// it has ingredients and buffer space. Time-driven via Tick(deltaSeconds)
    /// rather than Unity's clock, so production is fully unit tested.
    ///
    /// Ingredients are spent up front when a unit STARTS; the unit lands in the
    /// buffer after ProductionSeconds. Buffer capacity throttles it (and is the
    /// reason to come back and collect).
    /// </summary>
    public class ProducerStation
    {
        public BoosterType Output { get; }
        public TileType IngredientColor { get; }
        public int IngredientCost { get; }
        public float ProductionSeconds { get; }
        public int BufferCapacity { get; }

        public int BufferCount { get; private set; }
        public bool IsProducing { get; private set; }
        public float SecondsRemaining => IsProducing ? Mathf.Max(0f, ProductionSeconds - _progress) : 0f;
        public bool IsBufferFull => BufferCount >= BufferCapacity;

        public event Action Changed;

        private readonly IngredientInventory _ingredients;
        private float _progress;

        public ProducerStation(BoosterType output, TileType ingredientColor, int ingredientCost,
            float productionSeconds, int bufferCapacity, IngredientInventory ingredients)
        {
            if (ingredientColor == TileType.None) throw new ArgumentOutOfRangeException(nameof(ingredientColor));
            if (ingredientCost <= 0) throw new ArgumentOutOfRangeException(nameof(ingredientCost));
            if (productionSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(productionSeconds));
            if (bufferCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacity));

            Output = output;
            IngredientColor = ingredientColor;
            IngredientCost = ingredientCost;
            ProductionSeconds = productionSeconds;
            BufferCapacity = bufferCapacity;
            _ingredients = ingredients ?? throw new ArgumentNullException(nameof(ingredients));
        }

        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            if (!IsProducing)
            {
                TryStartProduction();
            }

            if (!IsProducing)
            {
                return;
            }

            _progress += deltaSeconds;
            if (_progress >= ProductionSeconds)
            {
                BufferCount++;
                IsProducing = false;
                _progress = 0f;
                Changed?.Invoke();
            }
        }

        /// <summary>Empties the buffer and returns how many boosters were collected.</summary>
        public int Collect()
        {
            int collected = BufferCount;
            if (collected > 0)
            {
                BufferCount = 0;
                Changed?.Invoke();
            }

            return collected;
        }

        private void TryStartProduction()
        {
            if (IsBufferFull)
            {
                return;
            }

            if (!_ingredients.TrySpend(IngredientColor, IngredientCost))
            {
                return;
            }

            IsProducing = true;
            _progress = 0f;
            Changed?.Invoke();
        }
    }
}