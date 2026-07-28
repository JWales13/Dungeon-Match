using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The player's stash of crafted boosters (collected from stations, spent
    /// when carried into a floor). Pure C#; persistence lives in a Gameplay
    /// repository. Mirrors IngredientInventory but keyed by BoosterType.
    /// </summary>
    public class BoosterInventory
    {
        private readonly Dictionary<BoosterType, int> _counts = new Dictionary<BoosterType, int>();

        public event Action Changed;

        public BoosterInventory(IReadOnlyDictionary<BoosterType, int> initial = null)
        {
            if (initial == null)
            {
                return;
            }

            foreach (KeyValuePair<BoosterType, int> entry in initial)
            {
                if (entry.Value > 0)
                {
                    _counts[entry.Key] = entry.Value;
                }
            }
        }

        public IReadOnlyDictionary<BoosterType, int> Counts => _counts;

        public int GetCount(BoosterType type)
        {
            return _counts.TryGetValue(type, out int count) ? count : 0;
        }

        public void Add(BoosterType type, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _counts[type] = GetCount(type) + amount;
            Changed?.Invoke();
        }

        public bool TrySpend(BoosterType type, int amount)
        {
            if (amount <= 0 || GetCount(type) < amount)
            {
                return false;
            }

            _counts[type] = GetCount(type) - amount;
            Changed?.Invoke();
            return true;
        }
    }
}