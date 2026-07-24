namespace Game.Core
{
    /// <summary>
    /// An immutable value representing a single board cell's contents.
    /// Immutability keeps board mutation logic centralized in Board instead of
    /// scattered across anything that happens to hold a reference to a tile.
    /// </summary>
    public readonly struct Tile
    {
        public static readonly Tile Empty = new Tile(TileType.None);

        public TileType Type { get; }
        public bool IsEmpty => Type == TileType.None;

        public Tile(TileType type)
        {
            Type = type;
        }
    }
}