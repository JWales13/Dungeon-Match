namespace Game.Core
{
    /// <summary>
    /// The set of tile "colors" the board can contain.
    /// None represents an empty cell and is never a valid, matchable tile.
    /// </summary>
    public enum TileType
    {
        None = -1,
        Red = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3
    }
}