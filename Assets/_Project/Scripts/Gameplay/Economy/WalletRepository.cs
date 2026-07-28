using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Saves/loads the Wallet as JSON (parallel arrays, like the other repositories).</summary>
    public class WalletRepository
    {
        [Serializable]
        private class SaveData
        {
            public int[] currencies = Array.Empty<int>();
            public int[] amounts = Array.Empty<int>();
        }

        private readonly string _filePath;

        public WalletRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "wallet.json");
        }

        public Wallet Load()
        {
            if (!File.Exists(_filePath))
            {
                return new Wallet();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));
                return ToWallet(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load wallet, starting empty: {e.Message}");
                return new Wallet();
            }
        }

        public void Save(Wallet wallet)
        {
            File.WriteAllText(_filePath, JsonUtility.ToJson(ToData(wallet)));
        }

        private static Wallet ToWallet(SaveData data)
        {
            var balances = new Dictionary<CurrencyType, int>();
            if (data.currencies != null && data.amounts != null)
            {
                int count = Mathf.Min(data.currencies.Length, data.amounts.Length);
                for (int i = 0; i < count; i++)
                {
                    balances[(CurrencyType)data.currencies[i]] = data.amounts[i];
                }
            }

            return new Wallet(balances);
        }

        private static SaveData ToData(Wallet wallet)
        {
            var currencies = new List<int>();
            var amounts = new List<int>();
            foreach (KeyValuePair<CurrencyType, int> entry in wallet.Balances)
            {
                currencies.Add((int)entry.Key);
                amounts.Add(entry.Value);
            }

            return new SaveData { currencies = currencies.ToArray(), amounts = amounts.ToArray() };
        }
    }
}