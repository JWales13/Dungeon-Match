using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Composes FloorDifficultyCurve (depth -> base stats) with
    /// FloorTierSchedule (depth -> tier) and each tier's TierMultipliers,
    /// producing the final FloorSpec GameController plays a floor with.
    /// Regular floors pass the base curve through unchanged; Main Event and
    /// Sweeps Week scale monster HP and Gold reward and set the ingredient
    /// harvest multiplier.
    /// </summary>
    public class TieredFloorGenerator
    {
        private readonly FloorDifficultyCurve _curve;
        private readonly FloorTierSchedule _schedule;
        private readonly TierMultipliers _mainEventMultipliers;
        private readonly TierMultipliers _sweepsWeekMultipliers;

        public TieredFloorGenerator(FloorDifficultyCurve curve, FloorTierSchedule schedule,
            TierMultipliers mainEventMultipliers, TierMultipliers sweepsWeekMultipliers)
        {
            _curve = curve ?? throw new ArgumentNullException(nameof(curve));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            _mainEventMultipliers = mainEventMultipliers;
            _sweepsWeekMultipliers = sweepsWeekMultipliers;
        }

        public FloorSpec Generate(int depth)
        {
            FloorSpec baseSpec = _curve.Generate(depth);
            FloorTier tier = _schedule.TierFor(depth);
            TierMultipliers multipliers = MultipliersFor(tier);

            int monsterHealth = Mathf.RoundToInt(baseSpec.MonsterHealth * multipliers.MonsterHealthMultiplier);
            int goldReward = Mathf.RoundToInt(baseSpec.GoldReward * multipliers.GoldRewardMultiplier);

            return new FloorSpec(depth, baseSpec.BoardSize, monsterHealth, baseSpec.MoveLimit, goldReward,
                tier, multipliers.IngredientMultiplier);
        }

        private TierMultipliers MultipliersFor(FloorTier tier)
        {
            switch (tier)
            {
                case FloorTier.MainEvent: return _mainEventMultipliers;
                case FloorTier.SweepsWeek: return _sweepsWeekMultipliers;
                default: return TierMultipliers.None;
            }
        }
    }
}
