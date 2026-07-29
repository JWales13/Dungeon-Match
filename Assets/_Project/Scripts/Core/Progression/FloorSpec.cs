using System;

namespace Game.Core
{
    /// <summary>
    /// One floor's generated parameters at a given depth: board size (always
    /// square), monster HP, move limit, and the Gold reward for clearing it.
    /// Pure data - FloorDifficultyCurve computes one of these per depth.
    /// </summary>
    public readonly struct FloorSpec
    {
        public int Depth { get; }
        public int BoardSize { get; }
        public int MonsterHealth { get; }
        public int MoveLimit { get; }
        public int GoldReward { get; }

        public FloorSpec(int depth, int boardSize, int monsterHealth, int moveLimit, int goldReward)
        {
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));
            if (boardSize <= 0) throw new ArgumentOutOfRangeException(nameof(boardSize));
            if (monsterHealth <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHealth));
            if (moveLimit <= 0) throw new ArgumentOutOfRangeException(nameof(moveLimit));
            if (goldReward <= 0) throw new ArgumentOutOfRangeException(nameof(goldReward));

            Depth = depth;
            BoardSize = boardSize;
            MonsterHealth = monsterHealth;
            MoveLimit = moveLimit;
            GoldReward = goldReward;
        }
    }
}
