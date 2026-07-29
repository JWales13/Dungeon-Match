using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Designer-tunable data for one level of a station's ladder. Mirrors
    /// StationLevelConfig field-for-field so it can round-trip through the
    /// Inspector; BuildDefinition() below converts a whole station's list of
    /// these into the pure-Core StationDefinition.
    /// </summary>
    [Serializable]
    public class StationLevelTuning
    {
        [Tooltip("How much of the station's ingredient one unit of production consumes.")]
        public int ingredientCost = 2;

        [Tooltip("How many real seconds one unit of production takes.")]
        public float productionSeconds = 10f;

        [Tooltip("How many finished boosters the station can hold before you collect.")]
        public int bufferCapacity = 2;

        [Tooltip("Prize Vouchers to reach this level (level 1's cost is the build cost).")]
        public int prizeVoucherCost = 10;
    }

    /// <summary>
    /// Designer-tunable data for one Producer station: its output booster, the
    /// ingredient color it consumes, and its level ladder (index 0 = level 1).
    /// </summary>
    [Serializable]
    public class StationTuning
    {
        [Tooltip("Shown on the station's panel (e.g. \"Bomb Bench\").")]
        public string displayName = "Station";

        public BoosterType output;
        public TileType ingredientColor;

        [Tooltip("If true and there's no save file yet, this station starts already built at Level 1 " +
                 "(use this for Bomb Bench, which predates the build/upgrade system).")]
        public bool startsBuilt;

        [Tooltip("Level 1 first, then each upgrade after it. Needs at least one entry.")]
        public List<StationLevelTuning> levels = new List<StationLevelTuning> { new StationLevelTuning() };

        public StationDefinition BuildDefinition()
        {
            var configs = new List<StationLevelConfig>(levels.Count);
            foreach (StationLevelTuning level in levels)
            {
                configs.Add(new StationLevelConfig(
                    level.ingredientCost, level.productionSeconds, level.bufferCapacity, level.prizeVoucherCost));
            }

            return new StationDefinition(output, ingredientColor, configs);
        }
    }

    /// <summary>
    /// The Green Room's station catalog: one entry per Producer station,
    /// authored in the Inspector rather than in code (same "data-driven
    /// catalog" pattern the old relic system used). Create via
    /// Assets/Create/DungeonVision/Station Catalog. GreenRoomController reads
    /// this once at Start to build the Core StationDefinitions.
    /// </summary>
    [CreateAssetMenu(fileName = "StationCatalog", menuName = "DungeonVision/Station Catalog")]
    public class StationCatalogAsset : ScriptableObject
    {
        public List<StationTuning> stations = new List<StationTuning>();
    }
}
