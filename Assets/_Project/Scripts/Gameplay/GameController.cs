using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Composition root and run orchestrator. Builds a Run from Inspector
    /// tuning, loads each room (board + combat objective + views), and reacts
    /// to room outcomes: advance to the next room, win the run, or lose it.
    /// Domain objects (Run, Board, MonsterCombatObjective) hold the rules;
    /// this class only wires and sequences them.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputController _inputController;
        [SerializeField] private CombatHudView _combatHudView;
        [SerializeField] private RunFlowView _runFlowView;

        [Header("Board size")]
        [SerializeField] private int _boardWidth = 8;
        [SerializeField] private int _boardHeight = 8;

        [Header("Run tuning")]
        [SerializeField] private int _roomCount = 4;
        [SerializeField] private int _baseMonsterHealth = 24;
        [SerializeField] private int _healthIncreasePerRoom = 10;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;

        private Run _run;
        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;
        private bool _acceptingInput;

        private void Start()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _runFlowView.ContinuePressed += HandleContinuePressed;
            _runFlowView.RestartPressed += HandleRestartPressed;

            StartNewRun();
        }

        private void OnDestroy()
        {
            if (_inputController != null)
            {
                _inputController.SwapRequested -= HandleSwapRequested;
            }

            if (_runFlowView != null)
            {
                _runFlowView.ContinuePressed -= HandleContinuePressed;
                _runFlowView.RestartPressed -= HandleRestartPressed;
            }

            UnsubscribeObjective();
        }

        private void StartNewRun()
        {
            _run = new Run(BuildRooms());
            _runFlowView.HideRunEnd();
            _runFlowView.HideBetweenRooms();
            LoadCurrentRoom();
        }

        private IReadOnlyList<RoomDefinition> BuildRooms()
        {
            var rooms = new List<RoomDefinition>();
            for (int i = 0; i < _roomCount; i++)
            {
                int health = _baseMonsterHealth + (i * _healthIncreasePerRoom);
                rooms.Add(new RoomDefinition(health, _moveLimit, _damagePerTile));
            }

            return rooms;
        }

        private void LoadCurrentRoom()
        {
            UnsubscribeObjective();

            RoomDefinition room = _run.CurrentRoom;
            _board = new Board(_boardWidth, _boardHeight, new MatchFinder());
            _objective = new MonsterCombatObjective(room.MonsterHealth, room.MoveLimit, room.DamagePerTile);
            _objectiveDriver = new BoardObjectiveDriver(_board, _objective);
            _objective.StatusChanged += HandleRoomStatusChanged;

            _boardView.Initialize(_board);
            _combatHudView.Initialize(_objective);
            _runFlowView.ShowRoomCounter(_run.CurrentRoomNumber, _run.TotalRooms);

            _acceptingInput = true;
        }

        private void HandleRoomStatusChanged(ObjectiveStatus status)
        {
            if (status == ObjectiveStatus.InProgress)
            {
                return;
            }

            _acceptingInput = false;
            bool roomWon = status == ObjectiveStatus.Won;
            _run.RegisterRoomResult(roomWon);

            switch (_run.Status)
            {
                case RunStatus.Won:
                    _runFlowView.ShowRunResult(won: true);
                    break;
                case RunStatus.Lost:
                    _runFlowView.ShowRunResult(won: false);
                    break;
                default:
                    // Run still in progress => we advanced to the next room.
                    _runFlowView.ShowBetweenRooms();
                    break;
            }
        }

        private void HandleContinuePressed()
        {
            _runFlowView.HideBetweenRooms();
            LoadCurrentRoom();
        }

        private void HandleRestartPressed()
        {
            StartNewRun();
        }

        private void HandleSwapRequested(Vector2Int a, Vector2Int b)
        {
            if (!_acceptingInput)
            {
                return;
            }

            _board.TrySwap(a, b);
        }

        private void UnsubscribeObjective()
        {
            if (_objective != null)
            {
                _objective.StatusChanged -= HandleRoomStatusChanged;
            }
        }
    }
}