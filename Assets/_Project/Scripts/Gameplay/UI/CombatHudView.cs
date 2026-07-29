using UnityEngine;
using TMPro;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// Displays monster HP, remaining moves, current tower depth, and the
    /// win/lose result. Every visual value (text color, font size, result
    /// strings and their colors) comes from the active Theme and is applied
    /// in code, so the HUD is styled without touching the TMP components in
    /// the Inspector.
    /// </summary>
    public class CombatHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _movesText;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _depthText;

        private MonsterCombatObjective _objective;

        public void Initialize(MonsterCombatObjective objective)
        {
            Unsubscribe();

            _objective = objective;
            ApplyTheme();

            _objective.HealthChanged += HandleHealthChanged;
            _objective.MovesChanged += HandleMovesChanged;
            _objective.StatusChanged += HandleStatusChanged;

            HandleHealthChanged(objective.CurrentHealth, objective.MaxHealth);
            HandleMovesChanged(objective.MovesRemaining, objective.MoveLimit);
            ClearResult();
        }

        /// <summary>Set once per floor load - depth/tier don't change mid-floor, so this isn't event-driven.</summary>
        public void SetDepth(int depth, FloorTier tier)
        {
            if (_depthText == null)
            {
                return;
            }

            switch (tier)
            {
                case FloorTier.MainEvent:
                    _depthText.text = $"Floor {depth} — MAIN EVENT";
                    break;
                case FloorTier.SweepsWeek:
                    _depthText.text = $"Floor {depth} — SWEEPS WEEK";
                    break;
                default:
                    _depthText.text = $"Floor {depth}";
                    break;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Detaches from the current objective so this HUD can be re-pointed at
        /// the next room's objective without stacking duplicate handlers.
        /// </summary>
        private void Unsubscribe()
        {
            if (_objective == null)
            {
                return;
            }

            _objective.HealthChanged -= HandleHealthChanged;
            _objective.MovesChanged -= HandleMovesChanged;
            _objective.StatusChanged -= HandleStatusChanged;
        }

        private void ApplyTheme()
        {
            ITheme theme = Theme.Current;

            StyleLabel(_healthText, theme.HudTextColor, theme.HudLabelFontSize);
            StyleLabel(_movesText, theme.HudTextColor, theme.HudLabelFontSize);
            StyleLabel(_depthText, theme.HudTextColor, theme.CaptionFontSize);

            if (_resultText != null)
            {
                _resultText.fontSize = theme.ResultFontSize;
            }
        }

        private static void StyleLabel(TMP_Text label, Color color, float fontSize)
        {
            if (label == null)
            {
                return;
            }

            label.color = color;
            label.fontSize = fontSize;
        }

        private void HandleHealthChanged(int current, int max)
        {
            _healthText.text = $"Monster HP: {current}/{max}";
        }

        private void HandleMovesChanged(int remaining, int limit)
        {
            _movesText.text = $"Moves: {remaining}";
        }

        private void HandleStatusChanged(ObjectiveStatus status)
        {
            if (_resultText == null)
            {
                return;
            }

            ITheme theme = Theme.Current;
            switch (status)
            {
                case ObjectiveStatus.Won:
                    _resultText.text = theme.VictoryMessage;
                    _resultText.color = theme.VictoryColor;
                    break;
                case ObjectiveStatus.Lost:
                    _resultText.text = theme.DefeatMessage;
                    _resultText.color = theme.DefeatColor;
                    break;
                default:
                    ClearResult();
                    break;
            }
        }

        private void ClearResult()
        {
            if (_resultText != null)
            {
                _resultText.text = string.Empty;
            }
        }
    }
}