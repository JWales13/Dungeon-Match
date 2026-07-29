using System;
using System.IO;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Saves/loads the current tower depth as a single int, same pattern as the other repositories.</summary>
    public class TowerProgressRepository
    {
        [Serializable]
        private class SaveData
        {
            public int depth = 1;
        }

        private readonly string _filePath;

        public TowerProgressRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "tower_progress.json");
        }

        public TowerProgress Load()
        {
            if (!File.Exists(_filePath))
            {
                return new TowerProgress();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));
                return new TowerProgress(Mathf.Max(1, data.depth));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load tower progress, starting at depth 1: {e.Message}");
                return new TowerProgress();
            }
        }

        public void Save(TowerProgress progress)
        {
            File.WriteAllText(_filePath, JsonUtility.ToJson(new SaveData { depth = progress.CurrentDepth }));
        }
    }
}
