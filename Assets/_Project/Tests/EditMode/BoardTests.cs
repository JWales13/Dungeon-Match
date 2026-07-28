using NUnit.Framework;
using UnityEngine;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Board-level invariants that hold regardless of power tiles: a clean
    /// start, adjacency rules, and no empty cells left after a move. All run in
    /// EditMode with no scene.
    /// </summary>
    public class BoardTests
    {
        [Test]
        public void Constructor_FillsGrid_WithNoStartingMatches()
        {
            var board = new Board(8, 8, new MatchFinder(), randomSeed: 1);

            var matches = new MatchFinder().FindMatches(GetGridSnapshot(board));

            Assert.AreEqual(0, matches.Count, "A freshly created board should never start with a match (line or square).");
        }

        [Test]
        public void TrySwap_NonAdjacentTiles_ReturnsFalse()
        {
            var board = new Board(8, 8, new MatchFinder(), randomSeed: 2);

            bool result = board.TrySwap(new Vector2Int(0, 0), new Vector2Int(2, 2));

            Assert.IsFalse(result);
        }

        [Test]
        public void TrySwap_NeverLeavesEmptyCells()
        {
            var board = new Board(8, 8, new MatchFinder(), randomSeed: 3);

            board.TrySwap(new Vector2Int(0, 0), new Vector2Int(1, 0));

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Assert.IsFalse(board.GetTile(new Vector2Int(x, y)).IsEmpty, $"Cell ({x},{y}) was left empty.");
                }
            }
        }

        private static Tile[,] GetGridSnapshot(Board board)
        {
            var grid = new Tile[board.Width, board.Height];
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    grid[x, y] = board.GetTile(new Vector2Int(x, y));
                }
            }

            return grid;
        }
    }
}