using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Power-tile creation and swap-to-detonate behavior, driven through Board
    /// with hand-built grids. Detonation results are captured from the
    /// TilesMatched event (fired with cells about to be cleared), so assertions
    /// don't depend on the random refill.
    /// </summary>
    public class PowerTileTests
    {
        private const int Seed = 12345;

        /// <summary>Checkerboard of two colors - has no runs and no squares.</summary>
        private static Tile[,] Checkerboard(int width, int height, TileType a = TileType.Red, TileType b = TileType.Blue)
        {
            var grid = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Tile((x + y) % 2 == 0 ? a : b);
                }
            }

            return grid;
        }

        private static List<Vector2Int> RecordClears(Board board)
        {
            var recorded = new List<Vector2Int>();
            board.TilesMatched += cells => recorded.AddRange(cells);
            return recorded;
        }

        [Test]
        public void HorizontalFourMatch_CreatesClearColumnPowerTile()
        {
            // Swapping (2,0)<->(2,1) turns row 0 into R R R R.
            var grid = new Tile[4, 3];
            SetRow(grid, 0, TileType.Red, TileType.Red, TileType.Blue, TileType.Red);
            SetRow(grid, 1, TileType.Green, TileType.Yellow, TileType.Red, TileType.Green);
            SetRow(grid, 2, TileType.Yellow, TileType.Green, TileType.Yellow, TileType.Blue);
            var board = new Board(grid, new MatchFinder(), Seed);

            bool swapped = board.TrySwap(new Vector2Int(2, 0), new Vector2Int(2, 1));

            Assert.IsTrue(swapped);
            Tile spawned = board.GetTile(new Vector2Int(2, 0));
            Assert.IsTrue(spawned.IsPowerTile);
            Assert.AreEqual(PowerTileKind.ClearColumn, spawned.Power);
        }

        [Test]
        public void MovingAColumnPowerTile_ClearsItsNewColumn()
        {
            var grid = Checkerboard(4, 4);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.ClearColumn);
            var board = new Board(grid, new MatchFinder(), Seed);
            var cleared = RecordClears(board);

            // Power tile moves from (1,1) to (2,1), then detonates on column x=2.
            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(2, 1));

            for (int y = 0; y < 4; y++)
            {
                Assert.Contains(new Vector2Int(2, y), cleared, $"Column cell (2,{y}) should have been cleared.");
            }
        }

        [Test]
        public void MovingAMortar_ClearsItselfAndAtLeastOneOtherTile()
        {
            var grid = Checkerboard(3, 3);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.Mortar);
            var board = new Board(grid, new MatchFinder(), Seed);
            var cleared = RecordClears(board);

            // Mortar moves to (1,0) and detonates there.
            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(1, 0));

            Assert.Contains(new Vector2Int(1, 0), cleared, "The mortar's own cell should clear.");
            Assert.GreaterOrEqual(cleared.Count, 2, "A mortar should clear its own cell plus a target.");
        }

        [Test]
        public void DetonationChains_ThroughAnotherPowerTile()
        {
            var grid = Checkerboard(4, 4);
            grid[2, 2] = new Tile(TileType.Green, PowerTileKind.ClearColumn);
            grid[2, 0] = new Tile(TileType.Yellow, PowerTileKind.ClearRow);
            var board = new Board(grid, new MatchFinder(), Seed);
            var cleared = RecordClears(board);

            // Move the column-clearer to (2,1); its column blast hits the
            // row-clearer at (2,0), which chains and clears row 0.
            board.TrySwap(new Vector2Int(2, 2), new Vector2Int(2, 1));

            for (int y = 0; y < 4; y++)
            {
                Assert.Contains(new Vector2Int(2, y), cleared, $"Column cell (2,{y}) should clear.");
            }

            for (int x = 0; x < 4; x++)
            {
                Assert.Contains(new Vector2Int(x, 0), cleared, $"Row cell ({x},0) should clear via the chain.");
            }
        }

        private static void SetRow(Tile[,] grid, int y, params TileType[] colors)
        {
            for (int x = 0; x < colors.Length; x++)
            {
                grid[x, y] = new Tile(colors[x]);
            }
        }
    }
}