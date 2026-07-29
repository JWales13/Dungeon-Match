using System;
using System.IO;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Persists a single UTC timestamp: the last moment the Green Room saved.
    /// GreenRoomController reads it at Start to compute how long the app was
    /// closed (for offline production catch-up via ProducerStation.FastForward),
    /// then overwrites it on every save.
    /// </summary>
    public class OfflineClockRepository
    {
        [Serializable]
        private class SaveData
        {
            public long utcTicks;
        }

        private readonly string _filePath;

        public OfflineClockRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "offline_clock.json");
        }

        /// <summary>The last saved timestamp, or null the very first time the Green Room ever saves.</summary>
        public DateTime? Load()
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));
                return new DateTime(data.utcTicks, DateTimeKind.Utc);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load offline clock, skipping catch-up: {e.Message}");
                return null;
            }
        }

        public void Save(DateTime utcNow)
        {
            File.WriteAllText(_filePath, JsonUtility.ToJson(new SaveData { utcTicks = utcNow.Ticks }));
        }
    }
}
