using System;

namespace Game.Core
{
    /// <summary>
    /// One floor's generated parameters at a given depth: board size (always
    /// square), monster HP, move limit, the Gold reward for clearing it, its
    /// difficulty Tier, and the ingredient-harvest multiplier that tier
    /// grants. FloorDifficultyCurve computes the base fields per depth;
    /// TieredFloorGenerator layers Tier/IngredientMultiplier on top (a
    /// Regular floor is Tier.Regular / multiplier 1).
    /// </summary>
    public readonly struct FloorSpec
    {
        public int Depth { get; }
        public int BoardSize { get; }
        public int MonsterHealth { get; }
        public int MoveLimit { get; }
        public int GoldReward { get; }
        public FloorTier Tier { get; }
        public int IngredientMultiplier { get; }

        public FloorSpec(int depth, int boardSize, int monsterHealth, int moveLimit, int goldReward,
            FloorTier tier, int ingredientMultiplier)
        {
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));
            if (boardSize <= 0) throw new ArgumentOutOfRangeException(nameof(boardSize));
            if (monsterHealth <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHealth));
            if (moveLimit <= 0) throw new ArgumentOutOfRangeException(nameof(moveLimit));
            if (goldReward <= 0) throw new ArgumentOutOfRangeException(nameof(goldReward));
            if (ingredientMultiplier < 1) throw new ArgumentOutOfRangeException(nameof(ingredientMultiplier));

            Depth = depth;
            BoardSize = boardSize;
            MonsterHealth = monsterHealth;
            MoveLimit = moveLimit;
            GoldReward = goldReward;
            Tier = tier;
            IngredientMultiplier = ingredientMultiplier;
        }
    }
}
