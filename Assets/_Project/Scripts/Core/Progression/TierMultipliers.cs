using System;

namespace Game.Core
{
    /// <summary>
    /// What a tier multiplies on top of the base depth curve: how much
    /// tougher the monster is (HP), how much extra Gold it pays, and how much
    /// ingredient harvest multiplies by. All multipliers are >= 1 - a tier
    /// only ever makes a floor harder/more rewarding, never easier.
    /// </summary>
    public readonly struct TierMultipliers
    {
        /// <summary>The (implicit) multipliers for a Regular floor - no change.</summary>
        public static readonly TierMultipliers None = new TierMultipliers(1f, 1f, 1);

        public float MonsterHealthMultiplier { get; }
        public float GoldRewardMultiplier { get; }
        public int IngredientMultiplier { get; }

        public TierMultipliers(float monsterHealthMultiplier, float goldRewardMultiplier, int ingredientMultiplier)
        {
            if (monsterHealthMultiplier < 1f) throw new ArgumentOutOfRangeException(nameof(monsterHealthMultiplier));
            if (goldRewardMultiplier < 1f) throw new ArgumentOutOfRangeException(nameof(goldRewardMultiplier));
            if (ingredientMultiplier < 1) throw new ArgumentOutOfRangeException(nameof(ingredientMultiplier));

            MonsterHealthMultiplier = monsterHealthMultiplier;
            GoldRewardMultiplier = goldRewardMultiplier;
            IngredientMultiplier = ingredientMultiplier;
        }
    }
}
