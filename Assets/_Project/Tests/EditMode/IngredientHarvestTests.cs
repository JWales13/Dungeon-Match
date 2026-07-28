using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// End-to-end harvest: detonating a power tile yields its color's
    /// ingredient (through the Board -> harvester -> inventory chain), and
    /// chains yield each detonated tile's color. Uses hand-built grids.
    /// </summary>
    public class IngredientHarvestTests
    {
        private const int Seed = 777;

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

        [Test]
        public void DetonatingAPowerTile_YieldsItsColorIngredient()
        {
            var grid = Checkerboard(4, 4);
            grid[1, 1] = new Tile(TileType.Green, PowerTileKind.ClearColumn);
            var board = new Board(grid, new MatchFinder(), Seed);
            var inventory = new IngredientInventory();
            _ = new IngredientHarvester(board, inventory, yieldPerDetonation: 2);

            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(2, 1));

            Assert.AreEqual(2, inventory.GetCount(TileType.Green));
        }

        [Test]
        public void ChainedDetonations_YieldEachColor()
        {
            var grid = Checkerboard(4, 4);
            grid[2, 2] = new Tile(TileType.Green, PowerTileKind.ClearColumn);
            grid[2, 0] = new Tile(TileType.Yellow, PowerTileKind.ClearRow);
            var board = new Board(grid, new MatchFinder(), Seed);
            var inventory = new IngredientInventory();
            _ = new IngredientHarvester(board, inventory, yieldPerDetonation: 1);

            // Column-clearer detonates and chains into the row-clearer.
            board.TrySwap(new Vector2Int(2, 2), new Vector2Int(2, 1));

            Assert.AreEqual(1, inventory.GetCount(TileType.Green));
            Assert.AreEqual(1, inventory.GetCount(TileType.Yellow));
        }

        [Test]
        public void PowerTileDetonated_EventFiresWithColor()
        {
            var grid = Checkerboard(3, 3);
            grid[1, 1] = new Tile(TileType.Red, PowerTileKind.ClearColumn);
            var board = new Board(grid, new MatchFinder(), Seed);
            var colors = new List<TileType>();
            board.PowerTileDetonated += colors.Add;

            board.TrySwap(new Vector2Int(1, 1), new Vector2Int(2, 1));

            Assert.Contains(TileType.Red, colors);
        }
    }
}