using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Game.Core
{
    /// <summary>
    /// Owns the grid of tiles and every rule for mutating it: swapping,
    /// clearing matches, collapsing gravity, and refilling. Pure C# - no
    /// MonoBehaviour, no rendering, no input. That's what makes it unit
    /// testable and safe to reuse if the presentation layer changes later.
    /// </summary>
    public class Board
    {
        public int Width { get; }
        public int Height { get; }

        /// <summary>Raised whenever two cells swap contents (including a reverted swap).</summary>
        public event Action<Vector2Int, Vector2Int> TilesSwapped;

        /// <summary>Raised once per cascade step with the cells that just matched.</summary>
        public event Action<IReadOnlyList<Vector2Int>> TilesMatched;

        /// <summary>Raised once all cascades from a swap have finished resolving.</summary>
        public event Action BoardSettled;

        private readonly Tile[,] _grid;
        private readonly IMatchFinder _matchFinder;
        private readonly Random _random;

        public Board(int width, int height, IMatchFinder matchFinder, int? randomSeed = null)
        {
            Width = width;
            Height = height;
            _matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
            _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            _grid = new Tile[width, height];

            FillWithoutStartingMatches();
        }

        public Tile GetTile(Vector2Int position)
        {
            ThrowIfOutOfBounds(position);
            return _grid[position.x, position.y];
        }

        public bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            int manhattanDistance = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            return manhattanDistance == 1;
        }

        /// <summary>
        /// Attempts to swap two adjacent cells. If the swap does not create a
        /// match, it is reverted and this returns false. Otherwise all
        /// resulting cascades are resolved before returning true.
        /// </summary>
        public bool TrySwap(Vector2Int a, Vector2Int b)
        {
            if (!AreAdjacent(a, b))
            {
                return false;
            }

            SwapTiles(a, b);
            TilesSwapped?.Invoke(a, b);

            var matches = _matchFinder.FindMatches(_grid);
            if (matches.Count == 0)
            {
                SwapTiles(a, b);
                TilesSwapped?.Invoke(a, b);
                return false;
            }

            ResolveMatches(matches);
            return true;
        }

        private void ResolveMatches(IReadOnlyList<Vector2Int> matches)
        {
            while (matches.Count > 0)
            {
                TilesMatched?.Invoke(matches);
                ClearTiles(matches);
                CollapseColumns();
                RefillEmptyTiles();
                matches = _matchFinder.FindMatches(_grid);
            }

            BoardSettled?.Invoke();
        }

        private void SwapTiles(Vector2Int a, Vector2Int b)
        {
            (_grid[a.x, a.y], _grid[b.x, b.y]) = (_grid[b.x, b.y], _grid[a.x, a.y]);
        }

        private void ClearTiles(IReadOnlyList<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                _grid[position.x, position.y] = Tile.Empty;
            }
        }

        private void CollapseColumns()
        {
            for (int x = 0; x < Width; x++)
            {
                CollapseColumn(x);
            }
        }

        private void CollapseColumn(int x)
        {
            int writeY = 0;
            for (int readY = 0; readY < Height; readY++)
            {
                if (_grid[x, readY].IsEmpty)
                {
                    continue;
                }

                _grid[x, writeY] = _grid[x, readY];
                writeY++;
            }

            for (int y = writeY; y < Height; y++)
            {
                _grid[x, y] = Tile.Empty;
            }
        }

        private void RefillEmptyTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y].IsEmpty)
                    {
                        _grid[x, y] = new Tile(RandomTileType());
                    }
                }
            }
        }

        private void FillWithoutStartingMatches()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _grid[x, y] = new Tile(RandomTypeAvoidingMatch(x, y));
                }
            }
        }

        private TileType RandomTypeAvoidingMatch(int x, int y)
        {
            TileType type;
            do
            {
                type = RandomTileType();
            }
            while (CreatesImmediateMatch(x, y, type));

            return type;
        }

        private bool CreatesImmediateMatch(int x, int y, TileType type)
        {
            bool horizontalMatch = x >= 2
                && _grid[x - 1, y].Type == type
                && _grid[x - 2, y].Type == type;

            bool verticalMatch = y >= 2
                && _grid[x, y - 1].Type == type
                && _grid[x, y - 2].Type == type;

            return horizontalMatch || verticalMatch;
        }

        private TileType RandomTileType()
        {
            int colorCount = Enum.GetValues(typeof(TileType)).Length - 1; // exclude None
            return (TileType)_random.Next(0, colorCount);
        }

        private void ThrowIfOutOfBounds(Vector2Int position)
        {
            bool outOfBounds = position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Height;
            if (outOfBounds)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }
        }
    }
}