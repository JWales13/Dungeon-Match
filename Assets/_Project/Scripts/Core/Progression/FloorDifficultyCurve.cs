using System;

namespace Game.Core
{
    /// <summary>
    /// Turns depth into a FloorSpec. Monster HP and Gold reward grow linearly
    /// per depth with no ceiling (an endless tower); move limit and board size
    /// grow the same way but are capped, so floors get harder without
    /// becoming unplayable or unbounded. Every coefficient is a constructor
    /// parameter (no magic numbers) - tuned from the Inspector, not edited
    /// here.
    /// </summary>
    public class FloorDifficultyCurve
    {
        private readonly int _baseMonsterHealth;
        private readonly int _monsterHealthPerDepth;
        private readonly int _baseMoveLimit;
        private readonly int _moveLimitPerDepth;
        private readonly int _maxMoveLimit;
        private readonly int _baseBoardSize;
        private readonly int _boardSizePerDepth;
        private readonly int _maxBoardSize;
        private readonly int _baseGoldReward;
        private readonly int _goldRewardPerDepth;

        public FloorDifficultyCurve(
            int baseMonsterHealth, int monsterHealthPerDepth,
            int baseMoveLimit, int moveLimitPerDepth, int maxMoveLimit,
            int baseBoardSize, int boardSizePerDepth, int maxBoardSize,
            int baseGoldReward, int goldRewardPerDepth)
        {
            if (baseMonsterHealth <= 0) throw new ArgumentOutOfRangeException(nameof(baseMonsterHealth));
            if (monsterHealthPerDepth < 0) throw new ArgumentOutOfRangeException(nameof(monsterHealthPerDepth));
            if (baseMoveLimit <= 0) throw new ArgumentOutOfRangeException(nameof(baseMoveLimit));
            if (moveLimitPerDepth < 0) throw new ArgumentOutOfRangeException(nameof(moveLimitPerDepth));
            if (maxMoveLimit < baseMoveLimit) throw new ArgumentOutOfRangeException(nameof(maxMoveLimit));
            if (baseBoardSize <= 0) throw new ArgumentOutOfRangeException(nameof(baseBoardSize));
            if (boardSizePerDepth < 0) throw new ArgumentOutOfRangeException(nameof(boardSizePerDepth));
            if (maxBoardSize < baseBoardSize) throw new ArgumentOutOfRangeException(nameof(maxBoardSize));
            if (baseGoldReward <= 0) throw new ArgumentOutOfRangeException(nameof(baseGoldReward));
            if (goldRewardPerDepth < 0) throw new ArgumentOutOfRangeException(nameof(goldRewardPerDepth));

            _baseMonsterHealth = baseMonsterHealth;
            _monsterHealthPerDepth = monsterHealthPerDepth;
            _baseMoveLimit = baseMoveLimit;
            _moveLimitPerDepth = moveLimitPerDepth;
            _maxMoveLimit = maxMoveLimit;
            _baseBoardSize = baseBoardSize;
            _boardSizePerDepth = boardSizePerDepth;
            _maxBoardSize = maxBoardSize;
            _baseGoldReward = baseGoldReward;
            _goldRewardPerDepth = goldRewardPerDepth;
        }

        public FloorSpec Generate(int depth)
        {
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));

            int depthIndex = depth - 1; // depth 1 = exactly the base values
            int monsterHealth = _baseMonsterHealth + _monsterHealthPerDepth * depthIndex;
            int moveLimit = Math.Min(_maxMoveLimit, _baseMoveLimit + _moveLimitPerDepth * depthIndex);
            int boardSize = Math.Min(_maxBoardSize, _baseBoardSize + _boardSizePerDepth * depthIndex);
            int goldReward = _baseGoldReward + _goldRewardPerDepth * depthIndex;

            return new FloorSpec(depth, boardSize, monsterHealth, moveLimit, goldReward);
        }
    }
}
