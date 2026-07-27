using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Default match rule: 3 or more same-type tiles in a row, horizontally or
    /// vertically. Each responsibility (scan horizontal, scan vertical, decide
    /// if a run counts, record a run) is its own small method.
    /// </summary>
    public class MatchFinder : IMatchFinder
    {
        private const int MinimumMatchLength = 3;

        public IReadOnlyList<Vector2Int> FindMatches(Tile[,] grid)
        {
            var matched = new HashSet<Vector2Int>();
            FindHorizontalMatches(grid, matched);
            FindVerticalMatches(grid, matched);
            return new List<Vector2Int>(matched);
        }

        private void FindHorizontalMatches(Tile[,] grid, HashSet<Vector2Int> matched)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= width; x++)
                {
                    bool runBroken = x == width || !SameType(grid, x, y, runStart, y);
                    if (!runBroken)
                    {
                        continue;
                    }

                    RecordRunIfLongEnough(matched, runStart, y, x - runStart, horizontal: true);
                    runStart = x;
                }
            }
        }

        private void FindVerticalMatches(Tile[,] grid, HashSet<Vector2Int> matched)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= height; y++)
                {
                    bool runBroken = y == height || !SameType(grid, x, y, x, runStart);
                    if (!runBroken)
                    {
                        continue;
                    }

                    RecordRunIfLongEnough(matched, x, runStart, y - runStart, horizontal: false);
                    runStart = y;
                }
            }
        }

        private static bool SameType(Tile[,] grid, int ax, int ay, int bx, int by)
        {
            Tile a = grid[ax, ay];
            Tile b = grid[bx, by];
            return !a.IsEmpty && a.Type == b.Type;
        }

        private static void RecordRunIfLongEnough(HashSet<Vector2Int> matched, int startX, int startY, int length, bool horizontal)
        {
            if (length < MinimumMatchLength)
            {
                return;
            }

            for (int i = 0; i < length; i++)
            {
                int x = horizontal ? startX + i : startX;
                int y = horizontal ? startY : startY + i;
                matched.Add(new Vector2Int(x, y));
            }
        }
    }
}