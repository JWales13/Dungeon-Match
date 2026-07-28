using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The in-floor booster loadout (Phase 4: just Dynamite). Shows how many
    /// Dynamite you hold and a Use button; raises UsePressed when tapped.
    /// GameController owns the arm/target/consume logic and drives this view's
    /// state via SetDynamite.
    /// </summary>
    public class BoosterLoadoutView : MonoBehaviour
    {
        [SerializeField] private Button _useDynamiteButton;
        [SerializeField] private TMP_Text _label;

        public event Action UseDynamitePressed;

        private void Awake()
        {
            if (_useDynamiteButton != null)
            {
                _useDynamiteButton.onClick.AddListener(RaiseUse);
            }

            if (_label != null)
            {
                ITheme theme = Theme.Current;
                _label.fontSize = theme.CaptionFontSize;
                _label.color = theme.HudTextColor;
            }
        }

        private void OnDestroy()
        {
            if (_useDynamiteButton != null)
            {
                _useDynamiteButton.onClick.RemoveListener(RaiseUse);
            }
        }

        /// <summary>Refreshes the display. When armed, prompts the player to tap a tile.</summary>
        public void SetDynamite(int count, bool usable, bool armed)
        {
            if (_label != null)
            {
                _label.text = armed ? "Tap a tile to blast!" : $"Dynamite: {count}";
            }

            if (_useDynamiteButton != null)
            {
                _useDynamiteButton.interactable = usable && !armed;
            }
        }

        private void RaiseUse()
        {
            UseDynamitePressed?.Invoke();
        }
    }
}