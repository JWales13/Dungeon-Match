namespace Game.Core
{
    /// <summary>
    /// An immutable board cell value: a color (Type), an optional power
    /// (Power), and optional crate durability (CrateHits). A normal tile has
    /// Power == None and CrateHits == 0. A power tile still carries a color
    /// but is inert to matching until the player moves it. A crate (CrateHits
    /// > 0) is likewise inert to matching/swapping - only a power-tile
    /// detonation or blast pattern can damage it (see Board.ResolveDetonation)
    /// - and survives until Damaged() brings CrateHits to 0.
    /// </summary>
    public readonly struct Tile
    {
        public static readonly Tile Empty = new Tile(TileType.None);

        public TileType Type { get; }
        public PowerTileKind Power { get; }
        public int CrateHits { get; }

        public bool IsEmpty => Type == TileType.None;
        public bool IsPowerTile => Power != PowerTileKind.None;
        public bool IsCrate => CrateHits > 0;

        public Tile(TileType type, PowerTileKind power = PowerTileKind.None, int crateHits = 0)
        {
            Type = type;
            Power = power;
            CrateHits = crateHits;
        }

        /// <summary>A copy of this crate with one fewer hit remaining (floored at 0).</summary>
        public Tile Damaged()
        {
            int remaining = CrateHits > 0 ? CrateHits - 1 : 0;
            return new Tile(Type, Power, remaining);
        }
    }
}