using UnityEngine;
using TMPro;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>Shows the player's currency balances (Gold, Prize Vouchers, Sponsor Bucks).</summary>
    public class CurrencyHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private static readonly CurrencyType[] DisplayOrder =
        {
            CurrencyType.Gold, CurrencyType.PrizeVoucher, CurrencyType.SponsorBucks
        };

        private Wallet _wallet;

        public void Initialize(Wallet wallet)
        {
            Unsubscribe();
            _wallet = wallet;
            ApplyStyle();
            _wallet.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_wallet != null)
            {
                _wallet.Changed -= Refresh;
            }
        }

        private void ApplyStyle()
        {
            if (_text == null)
            {
                return;
            }

            ITheme theme = Theme.Current;
            _text.fontSize = theme.CaptionFontSize;
            _text.color = theme.HudTextColor;
        }

        private void Refresh()
        {
            if (_text == null)
            {
                return;
            }

            ITheme theme = Theme.Current;
            var lines = new string[DisplayOrder.Length];
            for (int i = 0; i < DisplayOrder.Length; i++)
            {
                CurrencyType currency = DisplayOrder[i];
                lines[i] = $"{theme.GetCurrencyName(currency)}: {_wallet.GetBalance(currency)}";
            }

            _text.text = string.Join("\n", lines);
        }
    }
}