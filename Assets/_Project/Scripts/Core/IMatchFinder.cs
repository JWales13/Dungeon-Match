using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Finds matched tile groups on a grid. Extracted behind an interface so the
    /// matching algorithm can be swapped (e.g. add diagonal matches, L/T shapes)
    /// or mocked in tests without touching Board.
    /// </summary>
    public interface IMatchFinder
    {
        IReadOnlyList<Vector2Int> FindMatches(Tile[,] grid);
    }
}
