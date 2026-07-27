using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// Owns all run-flow UI: the persistent "Room X / Y" counter, the
    /// between-rooms panel (with a Continue button), and the run-end panel
    /// (with a Restart button). It only shows/hides and reports button presses;
    /// GameController decides what actually happens. Result strings/colors come
    /// from the theme.
    /// </summary>
    public class RunFlowView : MonoBehaviour
    {
        [Header("Always visible")]
        [SerializeField] private TMP_Text _roomCounterText;

        [Header("Between-rooms panel")]
        [SerializeField] private GameObject _betweenRoomsPanel;
        [SerializeField] private TMP_Text _betweenRoomsText;
        [SerializeField] private Button _continueButton;

        [Header("Run-end panel")]
        [SerializeField] private GameObject _runEndPanel;
        [SerializeField] private TMP_Text _runEndText;
        [SerializeField] private Button _restartButton;

        public event Action ContinuePressed;
        public event Action RestartPressed;

        private void Awake()
        {
            _continueButton.onClick.AddListener(RaiseContinue);
            _restartButton.onClick.AddListener(RaiseRestart);

            HideBetweenRooms();
            HideRunEnd();
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(RaiseContinue);
            _restartButton.onClick.RemoveListener(RaiseRestart);
        }

        public void ShowRoomCounter(int roomNumber, int totalRooms)
        {
            _roomCounterText.text = $"Room {roomNumber} / {totalRooms}";
        }

        public void ShowBetweenRooms()
        {
            _betweenRoomsText.text = Theme.Current.RoomClearedMessage;
            _betweenRoomsPanel.SetActive(true);
        }

        public void HideBetweenRooms()
        {
            _betweenRoomsPanel.SetActive(false);
        }

        public void ShowRunResult(bool won)
        {
            ITheme theme = Theme.Current;
            _runEndText.text = won ? theme.RunVictoryMessage : theme.RunDefeatMessage;
            _runEndText.color = won ? theme.VictoryColor : theme.DefeatColor;
            _runEndPanel.SetActive(true);
        }

        public void HideRunEnd()
        {
            _runEndPanel.SetActive(false);
        }

        private void RaiseContinue()
        {
            ContinuePressed?.Invoke();
        }

        private void RaiseRestart()
        {
            RestartPressed?.Invoke();
        }
    }
}