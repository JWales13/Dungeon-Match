namespace Game.Core
{
    /// <summary>
    /// What a power tile does when it detonates. Power tiles are created by
    /// large matches and set off when the player moves (swaps) them.
    /// </summary>
    public enum PowerTileKind
    {
        None = 0,
        ClearColumn, // from a horizontal 4-line: clears its entire column
        ClearRow,    // from a vertical 4-line: clears its entire row
        Mortar       // from a 2x2 square: strikes one objective tile, else a random tile
    }
}