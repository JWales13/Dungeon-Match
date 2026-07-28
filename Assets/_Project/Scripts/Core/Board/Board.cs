using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Game.Core
{
    /// <summary>
    /// Owns the grid and every rule for mutating it: swapping, creating power
    /// tiles from large matches, detonating power tiles when the player moves
    /// them (with chain reactions), clearing, gravity, and refill. Pure C# -
    /// no MonoBehaviour, no rendering, no input - so all of it is unit tested.
    ///
    /// Events (unchanged contract for the view/harvest layers):
    ///  - TilesSwapped(a, b): two cells exchanged (including a reverted swap).
    ///  - TilesMatched(cells): cells that are about to be cleared this step. It
    ///    fires BEFORE the cells are emptied, so listeners can read their colors
    ///    from the grid (used for damage + ingredient harvest). "Matched" now
    ///    means "cleared", covering both matches and power-tile detonations.
    ///  - BoardSettled: all cascades from a move have finished resolving.
    /// </summary>
    public class Board
    {
        public int Width { get; }
        public int Height { get; }

        public event Action<Vector2Int, Vector2Int> TilesSwapped;
        public event Action<IReadOnlyList<Vector2Int>> TilesMatched;
        public event Action BoardSettled;

        private static readonly IReadOnlyList<Vector2Int> NoCandidates = new List<Vector2Int>();

        private readonly Tile[,] _grid;
        private readonly IMatchFinder _matchFinder;
        private readonly Random _random;

        /// <summary>Creates a randomly filled board with no starting matches.</summary>
        public Board(int width, int height, IMatchFinder matchFinder, int? randomSeed = null)
        {
            Width = width;
            Height = height;
            _matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
            _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            _grid = new Tile[width, height];

            FillWithoutStartingMatches();
        }

        /// <summary>Creates a board from an explicit grid (used by tests).</summary>
        public Board(Tile[,] initialGrid, IMatchFinder matchFinder, int? randomSeed = null)
        {
            if (initialGrid == null) throw new ArgumentNullException(nameof(initialGrid));
            _matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
            _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            Width = initialGrid.GetLength(0);
            Height = initialGrid.GetLength(1);
            _grid = (Tile[,])initialGrid.Clone();
        }

        public Tile GetTile(Vector2Int position)
        {
            ThrowIfOutOfBounds(position);
            return _grid[position.x, position.y];
        }

        public bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }

        /// <summary>
        /// Swaps two adjacent cells. If either is a power tile, the swap always
        /// succeeds and the power tile detonates. Otherwise the swap must form a
        /// match, or it is reverted and this returns false.
        /// </summary>
        public bool TrySwap(Vector2Int a, Vector2Int b)
        {
            ThrowIfOutOfBounds(a);
            ThrowIfOutOfBounds(b);
            if (!AreAdjacent(a, b))
            {
                return false;
            }

            bool involvesPower = _grid[a.x, a.y].IsPowerTile || _grid[b.x, b.y].IsPowerTile;
            SwapCells(a, b);
            TilesSwapped?.Invoke(a, b);

            if (involvesPower)
            {
                var detonators = new List<Vector2Int>();
                if (_grid[a.x, a.y].IsPowerTile) detonators.Add(a);
                if (_grid[b.x, b.y].IsPowerTile) detonators.Add(b);
                ResolveDetonation(detonators);
                return true;
            }

            var groups = _matchFinder.FindMatches(_grid);
            if (groups.Count == 0)
            {
                SwapCells(a, b);
                TilesSwapped?.Invoke(a, b);
                return false;
            }

            RunMatchCascade(groups, new List<Vector2Int> { b, a });
            return true;
        }

        // --- Match resolution (may create power tiles) ---

        private void RunMatchCascade(IReadOnlyList<MatchGroup> groups, IReadOnlyList<Vector2Int> spawnCandidates)
        {
            while (groups.Count > 0)
            {
                ProcessMatchStep(groups, spawnCandidates);
                spawnCandidates = NoCandidates;
                groups = _matchFinder.FindMatches(_grid);
            }

            BoardSettled?.Invoke();
        }

        private void ProcessMatchStep(IReadOnlyList<MatchGroup> groups, IReadOnlyList<Vector2Int> spawnCandidates)
        {
            var spawns = new Dictionary<Vector2Int, Tile>();
            foreach (MatchGroup group in groups)
            {
                if (!TryGetPowerSpawn(group, out PowerTileKind kind))
                {
                    continue;
                }

                Vector2Int cell = ChooseSpawnCell(group, spawnCandidates, spawns);
                spawns[cell] = new Tile(group.Color, kind);
            }

            var cleared = new List<Vector2Int>();
            var clearedSet = new HashSet<Vector2Int>();
            foreach (MatchGroup group in groups)
            {
                foreach (Vector2Int cell in group.Cells)
                {
                    if (clearedSet.Add(cell))
                    {
                        cleared.Add(cell);
                    }
                }
            }

            if (cleared.Count > 0)
            {
                TilesMatched?.Invoke(cleared);
            }

            foreach (Vector2Int cell in cleared)
            {
                _grid[cell.x, cell.y] = Tile.Empty;
            }

            CollapseColumns();
            RefillEmptyTiles();

            // Stamp each created power tile onto its spawn cell (where the player
            // dropped the tile), overwriting whatever refilled there. Doing this
            // AFTER gravity keeps the power tile exactly at the swap spot instead
            // of falling through the gap left by its own (vertical) match.
            foreach (KeyValuePair<Vector2Int, Tile> spawn in spawns)
            {
                _grid[spawn.Key.x, spawn.Key.y] = spawn.Value;
            }
        }

        private static bool TryGetPowerSpawn(MatchGroup group, out PowerTileKind kind)
        {
            if (group.Shape == MatchShape.Square)
            {
                kind = PowerTileKind.Mortar;
                return true;
            }

            if (group.Shape == MatchShape.Line && group.Cells.Count >= 4)
            {
                kind = group.IsHorizontal ? PowerTileKind.ClearColumn : PowerTileKind.ClearRow;
                return true;
            }

            kind = PowerTileKind.None;
            return false;
        }

        private static Vector2Int ChooseSpawnCell(MatchGroup group, IReadOnlyList<Vector2Int> candidates, Dictionary<Vector2Int, Tile> taken)
        {
            foreach (Vector2Int candidate in candidates)
            {
                if (Contains(group.Cells, candidate) && !taken.ContainsKey(candidate))
                {
                    return candidate;
                }
            }

            Vector2Int middle = group.Cells[group.Cells.Count / 2];
            if (!taken.ContainsKey(middle))
            {
                return middle;
            }

            foreach (Vector2Int cell in group.Cells)
            {
                if (!taken.ContainsKey(cell))
                {
                    return cell;
                }
            }

            return middle;
        }

        private static bool Contains(IReadOnlyList<Vector2Int> cells, Vector2Int target)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == target)
                {
                    return true;
                }
            }

            return false;
        }

        // --- Detonation (chain reactions) ---

        private void ResolveDetonation(IReadOnlyList<Vector2Int> detonators)
        {
            var cleared = new List<Vector2Int>();
            var clearedSet = new HashSet<Vector2Int>();
            var toProcess = new Queue<Vector2Int>();
            foreach (Vector2Int cell in detonators)
            {
                toProcess.Enqueue(cell);
            }

            while (toProcess.Count > 0)
            {
                Vector2Int cell = toProcess.Dequeue();
                if (clearedSet.Contains(cell))
                {
                    continue;
                }

                Tile tile = _grid[cell.x, cell.y];
                if (tile.IsEmpty)
                {
                    continue;
                }

                clearedSet.Add(cell);
                cleared.Add(cell);

                if (tile.IsPowerTile)
                {
                    foreach (Vector2Int patternCell in ComputePattern(cell, tile.Power))
                    {
                        if (!clearedSet.Contains(patternCell))
                        {
                            toProcess.Enqueue(patternCell);
                        }
                    }
                }
            }

            if (cleared.Count > 0)
            {
                TilesMatched?.Invoke(cleared);
            }

            foreach (Vector2Int cell in cleared)
            {
                _grid[cell.x, cell.y] = Tile.Empty;
            }

            CollapseColumns();
            RefillEmptyTiles();

            RunMatchCascade(_matchFinder.FindMatches(_grid), NoCandidates);
        }

        private IEnumerable<Vector2Int> ComputePattern(Vector2Int origin, PowerTileKind kind)
        {
            switch (kind)
            {
                case PowerTileKind.ClearColumn:
                    for (int y = 0; y < Height; y++)
                    {
                        yield return new Vector2Int(origin.x, y);
                    }
                    break;

                case PowerTileKind.ClearRow:
                    for (int x = 0; x < Width; x++)
                    {
                        yield return new Vector2Int(x, origin.y);
                    }
                    break;

                case PowerTileKind.Mortar:
                    if (TryPickMortarTarget(origin, out Vector2Int target))
                    {
                        yield return target;
                    }
                    break;
            }
        }

        private bool TryPickMortarTarget(Vector2Int origin, out Vector2Int target)
        {
            // Objective tiles arrive in a later phase; for now, strike a random
            // non-empty tile other than the mortar's own cell.
            var candidates = new List<Vector2Int>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (cell == origin || _grid[x, y].IsEmpty)
                    {
                        continue;
                    }

                    candidates.Add(cell);
                }
            }

            if (candidates.Count == 0)
            {
                target = default;
                return false;
            }

            target = candidates[_random.Next(candidates.Count)];
            return true;
        }

        // --- Grid mechanics ---

        private void SwapCells(Vector2Int a, Vector2Int b)
        {
            (_grid[a.x, a.y], _grid[b.x, b.y]) = (_grid[b.x, b.y], _grid[a.x, a.y]);
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
            int guard = 0;
            do
            {
                type = RandomTileType();
                guard++;
            }
            while (CreatesImmediateMatch(x, y, type) && guard < 50);

            return type;
        }

        private bool CreatesImmediateMatch(int x, int y, TileType type)
        {
            bool horizontal = x >= 2
                && _grid[x - 1, y].Type == type
                && _grid[x - 2, y].Type == type;

            bool vertical = y >= 2
                && _grid[x, y - 1].Type == type
                && _grid[x, y - 2].Type == type;

            bool square = x >= 1 && y >= 1
                && _grid[x - 1, y].Type == type
                && _grid[x, y - 1].Type == type
                && _grid[x - 1, y - 1].Type == type;

            return horizontal || vertical || square;
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