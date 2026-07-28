using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The player's currency balances. Pure C#; persistence lives in a Gameplay
    /// repository. Same shape as the inventories, keyed by CurrencyType.
    /// </summary>
    public class Wallet
    {
        private readonly Dictionary<CurrencyType, int> _balances = new Dictionary<CurrencyType, int>();

        public event Action Changed;

        public Wallet(IReadOnlyDictionary<CurrencyType, int> initial = null)
        {
            if (initial == null)
            {
                return;
            }

            foreach (KeyValuePair<CurrencyType, int> entry in initial)
            {
                if (entry.Value > 0)
                {
                    _balances[entry.Key] = entry.Value;
                }
            }
        }

        public IReadOnlyDictionary<CurrencyType, int> Balances => _balances;

        public int GetBalance(CurrencyType currency)
        {
            return _balances.TryGetValue(currency, out int amount) ? amount : 0;
        }

        public void Add(CurrencyType currency, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _balances[currency] = GetBalance(currency) + amount;
            Changed?.Invoke();
        }

        public bool TrySpend(CurrencyType currency, int amount)
        {
            if (amount <= 0 || GetBalance(currency) < amount)
            {
                return false;
            }

            _balances[currency] = GetBalance(currency) - amount;
            Changed?.Invoke();
            return true;
        }
    }
}