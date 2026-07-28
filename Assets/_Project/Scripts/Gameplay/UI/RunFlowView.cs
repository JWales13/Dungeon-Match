using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// Single-floor result panel: shows VICTORY/ELIMINATED and a button to try
    /// a fresh floor. Trimmed in the Phase 0 cleanup to just the result flow
    /// (the old room-counter and between-rooms pieces belonged to the retired
    /// multi-room run). Null-safe so a missing reference logs nothing worse than
    /// an unresponsive button.
    /// </summary>
    public class RunFlowView : MonoBehaviour
    {
        [SerializeField] private GameObject _runEndPanel;
        [SerializeField] private TMP_Text _runEndText;
        [SerializeField] private Button _restartButton;

        public event Action RestartPressed;

        private void Awake()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(RaiseRestart);
            }

            HideRunEnd();
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(RaiseRestart);
            }
        }

        public void ShowRunResult(bool won)
        {
            if (_runEndText != null)
            {
                ITheme theme = Theme.Current;
                _runEndText.text = won ? theme.RunVictoryMessage : theme.RunDefeatMessage;
                _runEndText.color = won ? theme.VictoryColor : theme.DefeatColor;
            }

            if (_runEndPanel != null)
            {
                _runEndPanel.SetActive(true);
            }
        }

        public void HideRunEnd()
        {
            if (_runEndPanel != null)
            {
                _runEndPanel.SetActive(false);
            }
        }

        private void RaiseRestart()
        {
            RestartPressed?.Invoke();
        }
    }
}