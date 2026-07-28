using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Translates raw Board events into a per-move MoveOutcome and reports it to
    /// an IBoardObjective. Reads each matched tile's color during the match
    /// event (before the board clears it), so objectives/relics can award
    /// color- and size-based bonuses. One responsibility: build the outcome and
    /// report it.
    /// </summary>
    public class BoardObjectiveDriver
    {
        private readonly Board _board;
        private readonly IBoardObjective _objective;
        private readonly MoveOutcomeBuilder _builder = new MoveOutcomeBuilder();

        public BoardObjectiveDriver(Board board, IBoardObjective objective)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _objective = objective ?? throw new ArgumentNullException(nameof(objective));

            _board.TilesMatched += HandleTilesMatched;
            _board.BoardSettled += HandleBoardSettled;
        }

        private void HandleTilesMatched(IReadOnlyList<Vector2Int> matchedCells)
        {
            foreach (Vector2Int cell in matchedCells)
            {
                _builder.Add(_board.GetTile(cell).Type);
            }
        }

        private void HandleBoardSettled()
        {
            _objective.RegisterClears(_builder.Build());
            _builder.Reset();
        }
    }
}