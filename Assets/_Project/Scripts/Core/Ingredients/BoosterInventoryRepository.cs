using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Saves and loads the BoosterInventory as JSON in the platform's
    /// persistent data folder. Mirrors IngredientInventoryRepository (parallel
    /// arrays, since JsonUtility can't serialize dictionaries).
    /// </summary>
    public class BoosterInventoryRepository
    {
        [Serializable]
        private class SaveData
        {
            public int[] types = Array.Empty<int>();
            public int[] amounts = Array.Empty<int>();
        }

        private readonly string _filePath;

        public BoosterInventoryRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "boosters.json");
        }

        public BoosterInventory Load()
        {
            if (!File.Exists(_filePath))
            {
                return new BoosterInventory();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));
                return ToInventory(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load boosters, starting empty: {e.Message}");
                return new BoosterInventory();
            }
        }

        public void Save(BoosterInventory inventory)
        {
            File.WriteAllText(_filePath, JsonUtility.ToJson(ToData(inventory)));
        }

        private static BoosterInventory ToInventory(SaveData data)
        {
            var counts = new Dictionary<BoosterType, int>();
            if (data.types != null && data.amounts != null)
            {
                int count = Mathf.Min(data.types.Length, data.amounts.Length);
                for (int i = 0; i < count; i++)
                {
                    counts[(BoosterType)data.types[i]] = data.amounts[i];
                }
            }

            return new BoosterInventory(counts);
        }

        private static SaveData ToData(BoosterInventory inventory)
        {
            var types = new List<int>();
            var amounts = new List<int>();
            foreach (KeyValuePair<BoosterType, int> entry in inventory.Counts)
            {
                types.Add((int)entry.Key);
                amounts.Add(entry.Value);
            }

            return new SaveData { types = types.ToArray(), amounts = amounts.ToArray() };
        }
    }
}