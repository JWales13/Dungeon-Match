using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The player's persistent stash of crafting ingredients, one per tile
    /// color (Red=Gunpowder, Green=Toxic Goo, Yellow=Live Wire, Blue=Rations).
    /// Pure C#; file persistence lives in a Gameplay repository. Raises Changed
    /// so a HUD can refresh.
    /// </summary>
    public class IngredientInventory
    {
        private readonly Dictionary<TileType, int> _counts = new Dictionary<TileType, int>();

        public event Action Changed;

        public IngredientInventory(IReadOnlyDictionary<TileType, int> initial = null)
        {
            if (initial == null)
            {
                return;
            }

            foreach (KeyValuePair<TileType, int> entry in initial)
            {
                if (entry.Key != TileType.None && entry.Value > 0)
                {
                    _counts[entry.Key] = entry.Value;
                }
            }
        }

        public IReadOnlyDictionary<TileType, int> Counts => _counts;

        public int GetCount(TileType color)
        {
            return _counts.TryGetValue(color, out int count) ? count : 0;
        }

        public void Add(TileType color, int amount)
        {
            if (color == TileType.None || amount <= 0)
            {
                return;
            }

            _counts[color] = GetCount(color) + amount;
            Changed?.Invoke();
        }
    }
}