using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Saves/loads each station's level (0 = not built), keyed by the
    /// BoosterType it produces (one station = one booster, so that's a stable,
    /// unique key). Parallel-array JSON, same pattern as WalletRepository.
    ///
    /// Unlike the other repositories this doesn't hand back a ready-to-use
    /// domain object: a StationProgress needs a StationDefinition (from the
    /// catalog) and a Wallet to be constructed, neither of which the
    /// repository owns. So loading just returns level lookups; the caller
    /// (GreenRoomController) builds each StationProgress with the right
    /// initial level.
    /// </summary>
    public class StationProgressRepository
    {
        [Serializable]
        private class SaveData
        {
            public int[] boosterTypes = Array.Empty<int>();
            public int[] levels = Array.Empty<int>();
        }

        private readonly string _filePath;

        public StationProgressRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "stations.json");
        }

        /// <summary>Saved level per station's output BoosterType. Empty (not missing-per-key) if never saved.</summary>
        public IReadOnlyDictionary<BoosterType, int> LoadLevels()
        {
            if (!File.Exists(_filePath))
            {
                return new Dictionary<BoosterType, int>();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));
                return ToLevels(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load station progress, starting fresh: {e.Message}");
                return new Dictionary<BoosterType, int>();
            }
        }

        /// <summary>Only built stations (Level > 0) are written - an unbuilt station just has no entry.</summary>
        public void Save(IEnumerable<StationProgress> stations)
        {
            var boosterTypes = new List<int>();
            var levels = new List<int>();
            foreach (StationProgress station in stations)
            {
                if (!station.IsBuilt)
                {
                    continue;
                }

                boosterTypes.Add((int)station.Definition.Output);
                levels.Add(station.Level);
            }

            var data = new SaveData { boosterTypes = boosterTypes.ToArray(), levels = levels.ToArray() };
            File.WriteAllText(_filePath, JsonUtility.ToJson(data));
        }

        private static Dictionary<BoosterType, int> ToLevels(SaveData data)
        {
            var levels = new Dictionary<BoosterType, int>();
            if (data.boosterTypes != null && data.levels != null)
            {
                int count = Mathf.Min(data.boosterTypes.Length, data.levels.Length);
                for (int i = 0; i < count; i++)
                {
                    levels[(BoosterType)data.boosterTypes[i]] = data.levels[i];
                }
            }

            return levels;
        }
    }
}
