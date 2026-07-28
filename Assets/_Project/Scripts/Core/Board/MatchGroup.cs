using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// One detected match: the cells involved, their shared color, the shape,
    /// and (for lines) whether the run is horizontal. The Board decides which
    /// groups spawn power tiles, of what kind, and where.
    /// </summary>
    public sealed class MatchGroup
    {
        public IReadOnlyList<Vector2Int> Cells { get; }
        public TileType Color { get; }
        public MatchShape Shape { get; }
        public bool IsHorizontal { get; }

        public MatchGroup(IReadOnlyList<Vector2Int> cells, TileType color, MatchShape shape, bool isHorizontal)
        {
            Cells = cells ?? throw new ArgumentNullException(nameof(cells));
            Color = color;
            Shape = shape;
            IsHorizontal = isHorizontal;
        }
    }
}