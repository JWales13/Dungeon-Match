using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Default match rules: straight runs of 3+ (horizontal and vertical) and
    /// 2x2 squares of one color. Power tiles and empties are inert - they never
    /// form or extend a run/square - so a power tile just sits until moved.
    /// Each responsibility (scan a direction, record a run, find squares) is its
    /// own small method.
    /// </summary>
    public class MatchFinder : IMatchFinder
    {
        private const int MinimumRunLength = 3;

        public IReadOnlyList<MatchGroup> FindMatches(Tile[,] grid)
        {
            var groups = new List<MatchGroup>();
            FindHorizontalRuns(grid, groups);
            FindVerticalRuns(grid, groups);
            FindSquares(grid, groups);
            return groups;
        }

        private static bool IsMatchable(Tile tile) => !tile.IsEmpty && !tile.IsPowerTile;

        private static bool SameColor(Tile a, Tile b) => IsMatchable(a) && IsMatchable(b) && a.Type == b.Type;

        private void FindHorizontalRuns(Tile[,] grid, List<MatchGroup> groups)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= width; x++)
                {
                    bool runBroken = x == width || !SameColor(grid[x, y], grid[runStart, y]);
                    if (!runBroken)
                    {
                        continue;
                    }

                    AddRunIfLongEnough(grid, groups, runStart, y, x - runStart, horizontal: true);
                    runStart = x;
                }
            }
        }

        private void FindVerticalRuns(Tile[,] grid, List<MatchGroup> groups)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= height; y++)
                {
                    bool runBroken = y == height || !SameColor(grid[x, y], grid[x, runStart]);
                    if (!runBroken)
                    {
                        continue;
                    }

                    AddRunIfLongEnough(grid, groups, x, runStart, y - runStart, horizontal: false);
                    runStart = y;
                }
            }
        }

        private static void AddRunIfLongEnough(Tile[,] grid, List<MatchGroup> groups, int startX, int startY, int length, bool horizontal)
        {
            if (length < MinimumRunLength)
            {
                return;
            }

            var cells = new List<Vector2Int>(length);
            for (int i = 0; i < length; i++)
            {
                int x = horizontal ? startX + i : startX;
                int y = horizontal ? startY : startY + i;
                cells.Add(new Vector2Int(x, y));
            }

            TileType color = grid[cells[0].x, cells[0].y].Type;
            groups.Add(new MatchGroup(cells, color, MatchShape.Line, horizontal));
        }

        private void FindSquares(Tile[,] grid, List<MatchGroup> groups)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    Tile anchor = grid[x, y];
                    if (!IsMatchable(anchor))
                    {
                        continue;
                    }

                    bool isSquare = SameColor(anchor, grid[x + 1, y])
                        && SameColor(anchor, grid[x, y + 1])
                        && SameColor(anchor, grid[x + 1, y + 1]);

                    if (!isSquare)
                    {
                        continue;
                    }

                    var cells = new List<Vector2Int>
                    {
                        new Vector2Int(x, y),
                        new Vector2Int(x + 1, y),
                        new Vector2Int(x, y + 1),
                        new Vector2Int(x + 1, y + 1)
                    };
                    groups.Add(new MatchGroup(cells, anchor.Type, MatchShape.Square, isHorizontal: false));
                }
            }
        }
    }
}