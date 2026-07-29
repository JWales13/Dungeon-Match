using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The static catalog entry for one Producer station: what it makes, the
    /// ingredient color it consumes, and the stat/cost progression for each
    /// upgrade level (index 0 = level 1). Pure data - no runtime state.
    /// StationProgress tracks what a player has actually built/upgraded to.
    /// </summary>
    public class StationDefinition
    {
        public BoosterType Output { get; }
        public TileType IngredientColor { get; }
        public IReadOnlyList<StationLevelConfig> Levels { get; }

        public int MaxLevel => Levels.Count;

        public StationDefinition(BoosterType output, TileType ingredientColor, IReadOnlyList<StationLevelConfig> levels)
        {
            if (ingredientColor == TileType.None) throw new ArgumentOutOfRangeException(nameof(ingredientColor));
            if (levels == null || levels.Count == 0) throw new ArgumentException("A station needs at least one level.", nameof(levels));

            Output = output;
            IngredientColor = ingredientColor;
            Levels = levels;
        }

        /// <summary>1-based: GetLevel(1) is the config a station has right after being built.</summary>
        public StationLevelConfig GetLevel(int level)
        {
            if (level < 1 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
            return Levels[level - 1];
        }
    }
}
