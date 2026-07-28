using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// Shows the win/lose result for a single floor, with a button to try
    /// again. Renamed from RunFlowView - the old multi-room "run" is gone, so
    /// this is just the floor result panel now.
    /// </summary>
    public class FloorResultView : MonoBehaviour
    {
        [FormerlySerializedAs("_runEndPanel")]
        [SerializeField] private GameObject _resultPanel;

        [FormerlySerializedAs("_runEndText")]
        [SerializeField] private TMP_Text _resultText;

        [SerializeField] private Button _restartButton;

        public event Action PlayAgainPressed;

        private void Awake()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(RaisePlayAgain);
            }

            HideResult();
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(RaisePlayAgain);
            }
        }

        public void ShowResult(bool won)
        {
            if (_resultText != null)
            {
                ITheme theme = Theme.Current;
                _resultText.fontSize = theme.ResultFontSize;
                _resultText.text = won ? theme.FloorVictoryMessage : theme.FloorDefeatMessage;
                _resultText.color = won ? theme.VictoryColor : theme.DefeatColor;
            }

            if (_resultPanel != null)
            {
                _resultPanel.SetActive(true);
            }
        }

        public void HideResult()
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }
        }

        private void RaisePlayAgain()
        {
            PlayAgainPressed?.Invoke();
        }
    }
}