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
    /// ingredient / not built), booster stock, a Build/Upgrade button whose
    /// label and cost change with the station's state, and a single gem
    /// action button that context-switches between "Skip" (while brewing)
    /// and "Top Up" (while idle, needing its ingredient) - Sponsor Bucks
    /// sinks from GemExchange. Replaces the single-station StationView now
    /// that there are four of these.
    ///
    /// Purely a presenter - GreenRoomController owns the StationProgress and
    /// (once built) the ProducerStation, and drives this view via
    /// Initialize/SetProducer. This view never spends currency itself; the
    /// Build/Upgrade button raises AdvancePressed and lets the controller
    /// decide, while the gem action button calls straight into the shared
    /// GemExchange it's given (that's the one wallet-spending exception -
    /// GemExchange already owns the spend/effect pairing, so routing it back
    /// through the controller would just be an extra hop).
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
        [SerializeField] private Button _gemActionButton;
        [SerializeField] private TMP_Text _gemActionButtonLabel;

        public BoosterType StationOutput => _stationOutput;

        public event Action AdvancePressed;

        private StationProgress _progress;
        private ProducerStation _producer;
        private BoosterInventory _boosters;
        private Wallet _wallet;
        private GemExchange _gemExchange;
        private IngredientInventory _ingredients;

        private void Awake()
        {
            if (_advanceButton != null)
            {
                _advanceButton.onClick.AddListener(HandleAdvanceClicked);
            }

            if (_gemActionButton != null)
            {
                _gemActionButton.onClick.AddListener(HandleGemActionClicked);
            }
        }

        private void OnDestroy()
        {
            if (_advanceButton != null)
            {
                _advanceButton.onClick.RemoveListener(HandleAdvanceClicked);
            }

            if (_gemActionButton != null)
            {
                _gemActionButton.onClick.RemoveListener(HandleGemActionClicked);
            }

            Unsubscribe();
        }

        public void Initialize(string displayName, StationProgress progress, BoosterInventory boosters, Wallet wallet,
            GemExchange gemExchange, IngredientInventory ingredients)
        {
            Unsubscribe();

            _progress = progress;
            _boosters = boosters;
            _wallet = wallet;
            _gemExchange = gemExchange;
            _ingredients = ingredients;

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
            StyleLabel(_gemActionButtonLabel, theme);
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
            // The brewing countdown (and the skip cost it drives) needs a
            // per-frame refresh; everything else only changes on an event.
            RefreshStatus();
            RefreshGemAction();
        }

        private void RefreshAll()
        {
            RefreshStatus();
            RefreshStock();
            RefreshButton();
            RefreshGemAction();
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

        /// <summary>
        /// The gem button does one of two things depending on state: while
        /// brewing, it skips the timer; while idle (built, not brewing, buffer
        /// not full), it tops up the station's ingredient color. It's hidden
        /// otherwise (not built, or buffer full with nothing to skip/top up).
        /// </summary>
        private void RefreshGemAction()
        {
            if (_gemActionButton == null || _progress == null || _gemExchange == null)
            {
                return;
            }

            if (!_progress.IsBuilt || _producer == null)
            {
                SetGemAction(active: false);
                return;
            }

            if (_producer.IsProducing)
            {
                int cost = _gemExchange.SkipCost(_producer.SecondsRemaining);
                SetGemAction(active: true, $"Skip ({cost})", _wallet.GetBalance(CurrencyType.SponsorBucks) >= cost);
                return;
            }

            if (_producer.IsBufferFull)
            {
                SetGemAction(active: false);
                return;
            }

            int topUpCost = _gemExchange.IngredientTopUpCost;
            SetGemAction(active: true, $"Top Up ({topUpCost})", _wallet.GetBalance(CurrencyType.SponsorBucks) >= topUpCost);
        }

        private void SetGemAction(bool active, string label = null, bool interactable = false)
        {
            _gemActionButton.gameObject.SetActive(active);
            if (!active)
            {
                return;
            }

            _gemActionButton.interactable = interactable;
            if (_gemActionButtonLabel != null)
            {
                _gemActionButtonLabel.text = label;
            }
        }

        private void HandleGemActionClicked()
        {
            if (_gemExchange == null || _producer == null)
            {
                return;
            }

            if (_producer.IsProducing)
            {
                _gemExchange.TrySkipProduction(_producer);
            }
            else
            {
                _gemExchange.TryTopUpIngredients(_ingredients, _progress.Definition.IngredientColor);
            }
        }
    }
}
