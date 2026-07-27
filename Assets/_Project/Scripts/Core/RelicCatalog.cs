using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The demo's relic pool, defined in code (not ScriptableObject assets) so
    /// tuning lives with the logic and stays unit-testable. CreateDefault
    /// returns a fresh list of relic instances for a new run. To add a relic,
    /// add one line here - no Unity, no asset authoring.
    /// </summary>
    public static class RelicCatalog
    {
        public static IReadOnlyList<IRelic> CreateDefault()
        {
            return new List<IRelic>
            {
                new ExtraStartingMovesRelic("Adrenaline", extraMoves: 2),
                new ExtraStartingMovesRelic("Second Wind", extraMoves: 3),
                new FlatMoveDamageRelic("Brass Knuckles", bonus: 2),
                new FlatMoveDamageRelic("Sledgehammer", bonus: 4),
                new ColorTileBonusRelic("Bloodstone", TileType.Red, bonusPerTile: 1),
                new ColorTileBonusRelic("Sapphire Sigil", TileType.Blue, bonusPerTile: 1),
                new ColorTileBonusRelic("Emerald Charm", TileType.Green, bonusPerTile: 1),
                new ColorTileBonusRelic("Golden Idol", TileType.Yellow, bonusPerTile: 1),
                new BigMatchBonusRelic("Avalanche", threshold: 6, bonus: 5),
                new DamageMultiplierRelic("Overclock", percentBonus: 25)
            };
        }
    }
}