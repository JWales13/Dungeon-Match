using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Finds match groups on a grid (straight runs of 3+ and 2x2 squares).
    /// Behind an interface so the matching rules can be swapped or mocked
    /// without touching Board.
    /// </summary>
    public interface IMatchFinder
    {
        IReadOnlyList<MatchGroup> FindMatches(Tile[,] grid);
    }
}