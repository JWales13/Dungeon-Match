using System;

namespace Game.Core
{
    /// <summary>
    /// One station's build/upgrade state: Level 0 means not built. Spending
    /// Prize Vouchers to reach Level 1 builds it; spending again advances it
    /// further, up to the definition's max level. Owns its own Wallet
    /// spending, the same pattern ProducerStation uses for ingredient spend.
    /// </summary>
    public class StationProgress
    {
        public StationDefinition Definition { get; }
        public int Level { get; private set; }
        public bool IsBuilt => Level > 0;
        public bool IsMaxLevel => Level >= Definition.MaxLevel;

        /// <summary>The current level's production config, or null if not built yet.</summary>
        public StationLevelConfig? CurrentConfig => IsBuilt ? Definition.GetLevel(Level) : (StationLevelConfig?)null;

        /// <summary>Prize Voucher cost to build (Level 0) or upgrade to the next level. 0 if already maxed.</summary>
        public int NextCost => IsMaxLevel ? 0 : Definition.GetLevel(Level + 1).Cost;

        public bool CanAffordNext => !IsMaxLevel && _wallet.GetBalance(CurrencyType.PrizeVoucher) >= NextCost;

        public event Action Changed;

        private readonly Wallet _wallet;

        public StationProgress(StationDefinition definition, Wallet wallet, int initialLevel = 0)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            if (initialLevel < 0 || initialLevel > definition.MaxLevel) throw new ArgumentOutOfRangeException(nameof(initialLevel));

            Level = initialLevel;
        }

        /// <summary>
        /// Builds (if Level 0) or upgrades (if already built) to the next
        /// level, spending Prize Vouchers. Returns false, changing nothing, if
        /// already maxed or unaffordable.
        /// </summary>
        public bool TryAdvance()
        {
            if (IsMaxLevel)
            {
                return false;
            }

            int cost = Definition.GetLevel(Level + 1).Cost;
            if (!_wallet.TrySpend(CurrencyType.PrizeVoucher, cost))
            {
                return false;
            }

            Level++;
            Changed?.Invoke();
            return true;
        }
    }
}
