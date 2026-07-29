using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Crate obstacles: inert to plain swaps/matches, only damaged by a
    /// power-tile detonation or blast pattern passing over their cell, and
    /// only actually cleared once durability hits 0. Hand-built grids, same
    /// pattern as PowerTileTests.
    /// </summary>
    public class CrateTests
    {
        private const int Seed = 777;

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
        public void Tile_Damaged_DecrementsCrateHits_FlooredAtZero()
        {
            var crate = new Tile(TileType.Red, PowerTileKind.None, crateHits: 2);

            Tile oneHit = crate.Damaged();
            Assert.AreEqual(1, oneHit.CrateHits);
            Assert.IsTrue(oneHit.IsCrate);

            Tile destroyed = oneHit.Damaged();
            Assert.AreEqual(0, destroyed.CrateHits);
            Assert.IsFalse(destroyed.IsCrate);

            Tile stillZero = destroyed.Damaged(); // shouldn't go negative
            Assert.AreEqual(0, stillZero.CrateHits);
        }

        [Test]
        public void SwapInvolvingACrate_NeverForms_ARegularMatch()
        {
            // Three reds in a row would match, but the middle one is a crate.
            var grid = new Tile[3, 1];
            grid[0, 0] = new Tile(TileType.Red);
            grid[1, 0] = new Tile(TileType.Red, PowerTileKind.None, crateHits: 1);
            grid[2, 0] = new Tile(TileType.Blue);
            var board = new Board(grid, new MatchFinder(), Seed);

            // Swap the crate with its Blue neighbor - can't form a match either way.
            bool swapped = board.TrySwap(new Vector2Int(1, 0), new Vector2Int(2, 0));

            Assert.IsFalse(swapped);
        }

        [Test]
        public void RowClearPowerTile_DamagesCrate_ButDoesNotDestroyItInOneHit()
        {
            var grid = Checkerboard(4, 4);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.ClearRow);
            grid[2, 1] = new Tile(TileType.Blue, PowerTileKind.None, crateHits: 2);
            var board = new Board(grid, new MatchFinder(), Seed);
            var cleared = RecordClears(board);

            // Move the row-clearer from (1,1) to (0,1); detonates on row y=1, hitting the crate at (2,1).
            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(0, 1));

            Assert.IsFalse(cleared.Contains(new Vector2Int(2, 1)), "A crate with hits remaining should not be cleared.");
            Tile crateNow = board.GetTile(new Vector2Int(2, 1));
            Assert.IsTrue(crateNow.IsCrate);
            Assert.AreEqual(1, crateNow.CrateHits, "One hit from the row clear should have been spent.");
        }

        [Test]
        public void RowClearPowerTile_DestroysCrate_OnItsFinalHit()
        {
            var grid = Checkerboard(4, 4);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.ClearRow);
            grid[2, 1] = new Tile(TileType.Blue, PowerTileKind.None, crateHits: 1);
            var board = new Board(grid, new MatchFinder(), Seed);
            var cleared = RecordClears(board);

            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(0, 1));

            Assert.IsTrue(cleared.Contains(new Vector2Int(2, 1)), "A crate on its last hit should clear like any other cell.");
            Assert.IsFalse(board.GetTile(new Vector2Int(2, 1)).IsCrate, "The cell should have refilled with a normal tile.");
        }

        [Test]
        public void AreaBlast_DamagesCrateWithinItsRadius()
        {
            var grid = Checkerboard(3, 3);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.None, crateHits: 2);
            var board = new Board(grid, new MatchFinder(), Seed);

            board.UseAreaBlast(new Vector2Int(1, 1));

            // Everything else in the blast radius clears, so gravity can drop
            // the surviving crate to a new cell - assert on the tile itself,
            // not a fixed position.
            Tile crateNow = FindTheCrate(board);
            Assert.AreEqual(1, crateNow.CrateHits);
        }

        [Test]
        public void DamagedCrate_DoesNotChain_LikeAPowerTileWould()
        {
            // A crate sitting where a power tile's blast passes through should
            // just take a hit - it must not itself trigger further chaining
            // (it's never a power tile), and it must not be cleared while
            // hits remain.
            var grid = Checkerboard(4, 4);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.ClearColumn);
            grid[1, 2] = new Tile(TileType.Blue, PowerTileKind.None, crateHits: 5);
            var board = new Board(grid, new MatchFinder(), Seed);
            var cleared = RecordClears(board);

            // Column-clearer moves to (1,0), blasting column x=1 - including the crate at (1,2).
            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(1, 0));

            Assert.IsFalse(cleared.Contains(new Vector2Int(1, 2)));

            // The rest of column x=1 clears around it, so gravity can move
            // the crate - assert on the tile itself, not a fixed position.
            Tile crateNow = FindTheCrate(board);
            Assert.AreEqual(4, crateNow.CrateHits, "Exactly one hit should have been spent, not more.");
        }

        private static Tile FindTheCrate(Board board)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Tile tile = board.GetTile(new Vector2Int(x, y));
                    if (tile.IsCrate)
                    {
                        return tile;
                    }
                }
            }

            Assert.Fail("Expected exactly one surviving crate on the board.");
            return default;
        }
    }
}
