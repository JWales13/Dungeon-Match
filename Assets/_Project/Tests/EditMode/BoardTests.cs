using NUnit.Framework;
using UnityEngine;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// These run in the Unity Test Runner (EditMode) in milliseconds, with no
    /// scene, no play mode, and no rendering - because Board has zero
    /// dependency on any of that. This is the payoff of the Core/Gameplay
    /// split: the riskiest logic (match/cascade rules) is verifiable on its
    /// own.
    /// </summary>
    public class BoardTests
    {
        [Test]
        public void Constructor_FillsGrid_WithNoImmediateMatches()
        {
            var board = new Board(8, 8, new MatchFinder(), randomSeed: 1);

            var matches = new MatchFinder().FindMatches(GetGridSnapshot(board));

            Assert.AreEqual(0, matches.Count, "A freshly created board should never start with a pre-existing match.");
        }

        [Test]
        public void TrySwap_NonAdjacentTiles_ReturnsFalse()
        {
            var board = new Board(8, 8, new MatchFinder(), randomSeed: 2);

            bool result = board.TrySwap(new Vector2Int(0, 0), new Vector2Int(2, 2));

            Assert.IsFalse(result, "Only orthogonally adjacent cells should be swappable.");
        }

        [Test]
        public void TrySwap_NeverLeavesEmptyCellsOnTheBoard()
        {
            var board = new Board(8, 8, new MatchFinder(), randomSeed: 3);

            board.TrySwap(new Vector2Int(0, 0), new Vector2Int(1, 0));

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    bool isEmpty = board.GetTile(new Vector2Int(x, y)).IsEmpty;
                    Assert.IsFalse(isEmpty, $"Cell ({x},{y}) was left empty after a swap; refill logic should always backfill.");
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