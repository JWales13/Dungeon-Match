using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// A crafting station that turns one ingredient plus real time into one
    /// booster, held in a small buffer the player collects. Auto-produces while
    /// it has ingredients and buffer space. Time-driven via Tick(deltaSeconds)
    /// (one frame) or FastForward(totalSeconds) (a long stretch at once, e.g.
    /// offline catch-up) rather than Unity's clock, so production is fully
    /// unit tested.
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

        /// <param name="initialBufferCount">
        /// Seeds the buffer with boosters already produced - used when a station
        /// is upgraded and its ProducerStation instance is rebuilt with new
        /// stats, so boosters waiting for collection aren't lost. Must not
        /// exceed bufferCapacity.
        /// </param>
        public ProducerStation(BoosterType output, TileType ingredientColor, int ingredientCost,
            float productionSeconds, int bufferCapacity, IngredientInventory ingredients, int initialBufferCount = 0)
        {
            if (ingredientColor == TileType.None) throw new ArgumentOutOfRangeException(nameof(ingredientColor));
            if (ingredientCost <= 0) throw new ArgumentOutOfRangeException(nameof(ingredientCost));
            if (productionSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(productionSeconds));
            if (bufferCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacity));
            if (initialBufferCount < 0 || initialBufferCount > bufferCapacity) throw new ArgumentOutOfRangeException(nameof(initialBufferCount));

            Output = output;
            IngredientColor = ingredientColor;
            IngredientCost = ingredientCost;
            ProductionSeconds = productionSeconds;
            BufferCapacity = bufferCapacity;
            _ingredients = ingredients ?? throw new ArgumentNullException(nameof(ingredients));
            BufferCount = initialBufferCount;
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

        /// <summary>
        /// Simulates a long stretch of time all at once (how long the app was
        /// closed, say) rather than one frame. Unlike Tick, this loops so it
        /// can complete several units in a single call - still capped by
        /// buffer capacity and available ingredients, so it can't produce an
        /// unbounded backlog. Any leftover time once production stops (buffer
        /// full or out of ingredients) is simply not used. Used for offline
        /// production catch-up; ordinary per-frame updates should keep using
        /// Tick.
        /// </summary>
        public void FastForward(float totalSeconds)
        {
            float remaining = totalSeconds;
            while (remaining > 0f)
            {
                if (!IsProducing && !TryStartProduction())
                {
                    return;
                }

                float secondsToComplete = ProductionSeconds - _progress;
                if (remaining < secondsToComplete)
                {
                    _progress += remaining;
                    return;
                }

                remaining -= secondsToComplete;
                _progress = 0f;
                BufferCount++;
                IsProducing = false;
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

        private bool TryStartProduction()
        {
            if (IsBufferFull)
            {
                return false;
            }

            if (!_ingredients.TrySpend(IngredientColor, IngredientCost))
            {
                return false;
            }

            IsProducing = true;
            _progress = 0f;
            Changed?.Invoke();
            return true;
        }
    }
}