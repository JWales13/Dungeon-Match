using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// One station's panel in the Green Room: name, status (brewing / needs
    /// ingredient / not built), booster stock, and a single Build/Upgrade
    /// button whose label and cost change with the station's state. Replaces
    /// the single-station StationView now that there are four of these.
    ///
    /// Purely a presenter - GreenRoomController owns the StationProgress and
    /// (once built) the ProducerStation, and drives this view via
    /// Initialize/SetProducer. This view never spends Prize Vouchers itself;
    /// it only raises AdvancePressed and lets the controller decide.
    /// </summary>
    public class StationPanelView : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Must match one StationTuning.output entry in the Station Catalog asset.")]
        [SerializeField] private BoosterType _stationOutput;

        [Header("Scene references")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _stockText;
        [SerializeField] private Button _advanceButton;
        [SerializeField] private TMP_Text _advanceButtonLabel;

        public BoosterType StationOutput => _stationOutput;

        public event Action AdvancePressed;

        private StationProgress _progress;
        private ProducerStation _producer;
        private BoosterInventory _boosters;
        private Wallet _wallet;

        private void Awake()
        {
            if (_advanceButton != null)
            {
                _advanceButton.onClick.AddListener(HandleAdvanceClicked);
            }
        }

        private void OnDestroy()
        {
            if (_advanceButton != null)
            {
                _advanceButton.onClick.RemoveListener(HandleAdvanceClicked);
            }

            Unsubscribe();
        }

        public void Initialize(string displayName, StationProgress progress, BoosterInventory boosters, Wallet wallet)
        {
            Unsubscribe();

            _progress = progress;
            _boosters = boosters;
            _wallet = wallet;

            ApplyStyle();
            if (_nameText != null)
            {
                _nameText.text = displayName;
            }

            _progress.Changed += RefreshAll;
            _boosters.Changed += RefreshStock;
            _wallet.Changed += RefreshButton;

            RefreshAll();
        }

        /// <summary>Called by the controller whenever the built ProducerStation instance changes (built, upgraded, or torn down).</summary>
        public void SetProducer(ProducerStation producer)
        {
            _producer = producer;
            RefreshStatus();
        }

        private void Unsubscribe()
        {
            if (_progress != null)
            {
                _progress.Changed -= RefreshAll;
            }

            if (_boosters != null)
            {
                _boosters.Changed -= RefreshStock;
            }

            if (_wallet != null)
            {
                _wallet.Changed -= RefreshButton;
            }
        }

        private void ApplyStyle()
        {
            ITheme theme = Theme.Current;
            StyleLabel(_nameText, theme);
            StyleLabel(_statusText, theme);
            StyleLabel(_stockText, theme);
            StyleLabel(_advanceButtonLabel, theme);
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

        private void Update()
        {
            // The brewing countdown needs a per-frame refresh; everything else
            // only changes on an event.
            RefreshStatus();
        }

        private void RefreshAll()
        {
            RefreshStatus();
            RefreshStock();
            RefreshButton();
        }

        private void RefreshStatus()
        {
            if (_statusText == null || _progress == null)
            {
                return;
            }

            if (!_progress.IsBuilt)
            {
                _statusText.text = "Not built";
                return;
            }

            if (_producer == null)
            {
                _statusText.text = string.Empty;
                return;
            }

            _statusText.text = _producer.IsProducing
                ? $"Brewing {Mathf.CeilToInt(_producer.SecondsRemaining)}s"
                : $"Needs {Theme.Current.GetIngredientName(_producer.IngredientColor)}";
        }

        private void RefreshStock()
        {
            if (_stockText == null || _progress == null)
            {
                return;
            }

            string name = Theme.Current.GetBoosterName(_progress.Definition.Output);
            _stockText.text = $"{name}: {_boosters.GetCount(_progress.Definition.Output)}";
        }

        private void RefreshButton()
        {
            if (_progress == null)
            {
                return;
            }

            if (_advanceButton != null)
            {
                _advanceButton.interactable = !_progress.IsMaxLevel && _progress.CanAffordNext;
            }

            if (_advanceButtonLabel != null)
            {
                _advanceButtonLabel.text = _progress.IsMaxLevel
                    ? "Max level"
                    : $"{(_progress.IsBuilt ? "Upgrade" : "Build")} ({_progress.NextCost})";
            }
        }

        private void HandleAdvanceClicked()
        {
            AdvancePressed?.Invoke();
        }
    }
}
