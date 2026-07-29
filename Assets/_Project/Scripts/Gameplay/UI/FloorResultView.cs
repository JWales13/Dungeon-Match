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
    /// Voucher, ingredients harvested); on a loss it shows the defeat banner.
    /// For now both paths use one Exit button (Continue/Retry arrive in 5a-3b).
    /// A presenter only - it displays and raises button events.
    /// </summary>
    public class FloorResultView : MonoBehaviour
    {
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _rewardsText;

        [FormerlySerializedAs("_restartButton")]
        [SerializeField] private Button _exitButton;

        public event Action ExitPressed;

        private void Awake()
        {
            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(RaiseExit);
            }

            HideResult();
        }

        private void OnDestroy()
        {
            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(RaiseExit);
            }
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

            ShowPanel();
        }

        public void ShowFail()
        {
            ITheme theme = Theme.Current;

            SetResultText(theme.FloorDefeatMessage, theme.DefeatColor);

            if (_rewardsText != null)
            {
                _rewardsText.text = string.Empty;
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

        private void RaiseExit()
        {
            ExitPressed?.Invoke();
        }
    }
}