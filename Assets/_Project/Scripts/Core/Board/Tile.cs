namespace Game.Core
{
    /// <summary>
    /// An immutable board cell value: a color (Type) and an optional power
    /// (Power). A normal tile has Power == None. A power tile still carries a
    /// color but is inert to matching until the player moves it.
    /// </summary>
    public readonly struct Tile
    {
        public static readonly Tile Empty = new Tile(TileType.None);

        public TileType Type { get; }
        public PowerTileKind Power { get; }

        public bool IsEmpty => Type == TileType.None;
        public bool IsPowerTile => Power != PowerTileKind.None;

        public Tile(TileType type, PowerTileKind power = PowerTileKind.None)
        {
            Type = type;
            Power = power;
        }
    }
}