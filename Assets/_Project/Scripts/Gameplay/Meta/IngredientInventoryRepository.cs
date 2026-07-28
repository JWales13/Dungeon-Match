using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Saves and loads the IngredientInventory as JSON in the platform's
    /// persistent data folder. The only place that knows how the stash is
    /// stored, so the pure IngredientInventory stays free of serialization.
    /// JsonUtility can't serialize dictionaries, so we map to parallel arrays.
    /// </summary>
    public class IngredientInventoryRepository
    {
        [Serializable]
        private class SaveData
        {
            public int[] colors = Array.Empty<int>();
            public int[] amounts = Array.Empty<int>();
        }

        private readonly string _filePath;

        public IngredientInventoryRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "ingredients.json");
        }

        public IngredientInventory Load()
        {
            if (!File.Exists(_filePath))
            {
                return new IngredientInventory();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));
                return ToInventory(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load ingredients, starting empty: {e.Message}");
                return new IngredientInventory();
            }
        }

        public void Save(IngredientInventory inventory)
        {
            File.WriteAllText(_filePath, JsonUtility.ToJson(ToData(inventory)));
        }

        private static IngredientInventory ToInventory(SaveData data)
        {
            var counts = new Dictionary<TileType, int>();
            if (data.colors != null && data.amounts != null)
            {
                int count = Mathf.Min(data.colors.Length, data.amounts.Length);
                for (int i = 0; i < count; i++)
                {
                    counts[(TileType)data.colors[i]] = data.amounts[i];
                }
            }

            return new IngredientInventory(counts);
        }

        private static SaveData ToData(IngredientInventory inventory)
        {
            var colors = new List<int>();
            var amounts = new List<int>();
            foreach (KeyValuePair<TileType, int> entry in inventory.Counts)
            {
                colors.Add((int)entry.Key);
                amounts.Add(entry.Value);
            }

            return new SaveData { colors = colors.ToArray(), amounts = amounts.ToArray() };
        }
    }
}