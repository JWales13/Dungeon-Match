using System;

namespace Game.Core
{
    /// <summary>Adds a flat number of moves to every room in the run.</summary>
    public class ExtraStartingMovesRelic : Relic
    {
        private readonly int _extraMoves;

        public override string DisplayName { get; }
        public override string Description => $"+{_extraMoves} moves each room.";

        public ExtraStartingMovesRelic(string displayName, int extraMoves)
        {
            if (extraMoves <= 0) throw new ArgumentOutOfRangeException(nameof(extraMoves));
            DisplayName = displayName;
            _extraMoves = extraMoves;
        }

        public override int ModifyMoveLimit(int baseMoveLimit) => baseMoveLimit + _extraMoves;
    }
}