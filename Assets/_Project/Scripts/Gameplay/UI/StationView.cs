using UnityEngine;
using TMPro;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The Bomb Bench panel: shows what the station is doing (brewing timer or
    /// waiting on ingredients) and the current booster stock. A read-only
    /// presenter - collection is automatic (GameController auto-collects), so
    /// there is no button.
    /// </summary>
    public class StationView : MonoBehaviour
    {
        [SerializeField] private string _stationName = "Bomb Bench";
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _stockText;

        private ProducerStation _station;
        private BoosterInventory _boosters;

        public void Initialize(ProducerStation station, BoosterInventory boosters)
        {
            _station = station;
            _boosters = boosters;

            ApplyStyle();
            _boosters.Changed += RefreshStock;
            RefreshStock();
        }

        private void ApplyStyle()
        {
            ITheme theme = Theme.Current;
            StyleLabel(_statusText, theme);
            StyleLabel(_stockText, theme);
        }

        private static void StyleLabel(TMP_Text label, ITheme theme)
        {
            if (label == null)
            {
                return;
            }

            label.fontSize = theme.CaptionFontSize;
            label.color = theme.HudTextColor;
        }

        private void OnDestroy()
        {
            if (_boosters != null)
            {
                _boosters.Changed -= RefreshStock;
            }
        }

        private void Update()
        {
            if (_station == null || _statusText == null)
            {
                return;
            }

            _statusText.text = BuildStatusLine();
        }

        private string BuildStatusLine()
        {
            if (_station.IsProducing)
            {
                return $"{_stationName}: brewing {Mathf.CeilToInt(_station.SecondsRemaining)}s";
            }

            return $"{_stationName}: needs {Theme.Current.GetIngredientName(_station.IngredientColor)}";
        }

        private void RefreshStock()
        {
            if (_stockText == null || _station == null)
            {
                return;
            }

            string name = Theme.Current.GetBoosterName(_station.Output);
            _stockText.text = $"{name}: {_boosters.GetCount(_station.Output)}";
        }
    }
}