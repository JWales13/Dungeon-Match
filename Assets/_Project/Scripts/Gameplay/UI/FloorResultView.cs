using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The floor result panel. On a win it shows the rewards (Gold, Prize
    /// Voucher, ingredients harvested) with an Exit button. On a loss it shows
    /// the defeat banner plus Continue (paid, +moves), Retry, and Exit. A
    /// presenter only - it displays and raises button events.
    /// </summary>
    public class FloorResultView : MonoBehaviour
    {
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _rewardsText;

        [FormerlySerializedAs("_restartButton")]
        [SerializeField] private Button _exitButton;

        [Header("Fail options")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _continueLabel;
        [SerializeField] private Button _retryButton;

        public event Action ExitPressed;
        public event Action ContinuePressed;
        public event Action RetryPressed;

        private void Awake()
        {
            AddListener(_exitButton, RaiseExit);
            AddListener(_continueButton, RaiseContinue);
            AddListener(_retryButton, RaiseRetry);
            HideResult();
        }

        private void OnDestroy()
        {
            RemoveListener(_exitButton, RaiseExit);
            RemoveListener(_continueButton, RaiseContinue);
            RemoveListener(_retryButton, RaiseRetry);
        }

        public void ShowWin(int goldEarned, int vouchersEarned, string harvestSummary)
        {
            ITheme theme = Theme.Current;
            SetResultText(theme.FloorVictoryMessage, theme.VictoryColor);

            if (_rewardsText != null)
            {
                _rewardsText.fontSize = theme.CaptionFontSize;
                _rewardsText.color = theme.HudTextColor;
                string rewards = $"Gold +{goldEarned}\nPrize Voucher +{vouchersEarned}";
                if (!string.IsNullOrEmpty(harvestSummary))
                {
                    rewards += $"\nHarvested: {harvestSummary}";
                }

                _rewardsText.text = rewards;
            }

            SetFailOptionsVisible(false);
            ShowPanel();
        }

        public void ShowFail(int continueCost, bool canAfford)
        {
            ITheme theme = Theme.Current;
            SetResultText(theme.FloorDefeatMessage, theme.DefeatColor);

            if (_rewardsText != null)
            {
                _rewardsText.text = string.Empty;
            }

            SetFailOptionsVisible(true);

            if (_continueLabel != null)
            {
                _continueLabel.text = $"Continue (+5)\n{continueCost} Gold";
            }

            if (_continueButton != null)
            {
                _continueButton.interactable = canAfford;
            }

            ShowPanel();
        }

        public void HideResult()
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }
        }

        private void SetFailOptionsVisible(bool visible)
        {
            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(visible);
            }

            if (_retryButton != null)
            {
                _retryButton.gameObject.SetActive(visible);
            }
        }

        private void SetResultText(string message, Color color)
        {
            if (_resultText == null)
            {
                return;
            }

            _resultText.fontSize = Theme.Current.ResultFontSize;
            _resultText.text = message;
            _resultText.color = color;
        }

        private void ShowPanel()
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(true);
            }
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private void RaiseExit() => ExitPressed?.Invoke();
        private void RaiseContinue() => ContinuePressed?.Invoke();
        private void RaiseRetry() => RetryPressed?.Invoke();
    }
}