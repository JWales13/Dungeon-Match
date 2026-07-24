using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Translates raw Board events into "a move fully resolved, N tiles were
    /// cleared" calls on an IBoardObjective. This is the single wiring point
    /// between the grid and the win/lose rule, so neither has to know about
    /// the other. One responsibility: count tiles per move and report it.
    /// </summary>
    public class BoardObjectiveDriver
    {
        private readonly IBoardObjective _objective;
        private int _tilesClearedThisMove;

        public BoardObjectiveDriver(Board board, IBoardObjective objective)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            _objective = objective ?? throw new ArgumentNullException(nameof(objective));

            board.TilesMatched += HandleTilesMatched;
            board.BoardSettled += HandleBoardSettled;
        }

        private void HandleTilesMatched(IReadOnlyList<Vector2Int> matchedCells)
        {
            _tilesClearedThisMove += matchedCells.Count;
        }

        private void HandleBoardSettled()
        {
            _objective.RegisterResolvedMove(_tilesClearedThisMove);
            _tilesClearedThisMove = 0;
        }
    }
}