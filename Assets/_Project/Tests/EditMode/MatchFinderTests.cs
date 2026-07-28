using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Match-group detection on hand-built grids: run lengths, orientation, and
    /// squares. Grids are indexed [x, y]; unset cells are a filler color that
    /// won't accidentally match.
    /// </summary>
    public class MatchFinderTests
    {
        private static Tile[,] Grid(int width, int height, TileType fill = TileType.None)
        {
            var grid = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = fill == TileType.None ? Tile.Empty : new Tile(fill);
                }
            }

            return grid;
        }

        [Test]
        public void HorizontalFour_IsOneHorizontalLineGroup()
        {
            var grid = Grid(5, 1);
            for (int x = 0; x < 4; x++) grid[x, 0] = new Tile(TileType.Red);
            grid[4, 0] = new Tile(TileType.Blue);

            IReadOnlyList<MatchGroup> groups = new MatchFinder().FindMatches(grid);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line, groups[0].Shape);
            Assert.IsTrue(groups[0].IsHorizontal);
            Assert.AreEqual(4, groups[0].Cells.Count);
        }

        [Test]
        public void VerticalFour_IsOneVerticalLineGroup()
        {
            var grid = Grid(1, 5);
            for (int y = 0; y < 4; y++) grid[0, y] = new Tile(TileType.Green);
            grid[0, 4] = new Tile(TileType.Yellow);

            IReadOnlyList<MatchGroup> groups = new MatchFinder().FindMatches(grid);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line, groups[0].Shape);
            Assert.IsFalse(groups[0].IsHorizontal);
            Assert.AreEqual(4, groups[0].Cells.Count);
        }

        [Test]
        public void TwoByTwoBlock_IsOneSquareGroup()
        {
            var grid = Grid(2, 2);
            grid[0, 0] = new Tile(TileType.Blue);
            grid[1, 0] = new Tile(TileType.Blue);
            grid[0, 1] = new Tile(TileType.Blue);
            grid[1, 1] = new Tile(TileType.Blue);

            IReadOnlyList<MatchGroup> groups = new MatchFinder().FindMatches(grid);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Square, groups[0].Shape);
            Assert.AreEqual(4, groups[0].Cells.Count);
        }

        [Test]
        public void PowerTilesAreInertToMatching()
        {
            // Three reds in a row, but the middle one is a power tile -> no run.
            var grid = Grid(3, 1);
            grid[0, 0] = new Tile(TileType.Red);
            grid[1, 0] = new Tile(TileType.Red, PowerTileKind.ClearColumn);
            grid[2, 0] = new Tile(TileType.Red);

            IReadOnlyList<MatchGroup> groups = new MatchFinder().FindMatches(grid);

            Assert.AreEqual(0, groups.Count);
        }
    }
}