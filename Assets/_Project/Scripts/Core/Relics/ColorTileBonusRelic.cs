using System;

namespace Game.Core
{
    /// <summary>Each cleared tile of a specific color adds bonus damage.</summary>
    public class ColorTileBonusRelic : Relic
    {
        private readonly TileType _color;
        private readonly int _bonusPerTile;

        public override string DisplayName { get; }
        public override string Description => $"Each {_color} tile cleared deals +{_bonusPerTile} damage.";

        public ColorTileBonusRelic(string displayName, TileType color, int bonusPerTile)
        {
            if (color == TileType.None) throw new ArgumentOutOfRangeException(nameof(color));
            if (bonusPerTile <= 0) throw new ArgumentOutOfRangeException(nameof(bonusPerTile));
            DisplayName = displayName;
            _color = color;
            _bonusPerTile = bonusPerTile;
        }

        public override int ModifyMoveDamage(int baseDamage, MoveOutcome move)
        {
            return baseDamage + (move.CountOf(_color) * _bonusPerTile);
        }
    }
}