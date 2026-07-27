using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Composition root and run orchestrator. Builds a Run from Inspector
    /// tuning, loads each room (board + relic-aware combat objective + views),
    /// and drives the flow: on a room win, offer a relic reward and advance;
    /// on the final win, win the run; on any loss, end it. Domain objects hold
    /// the rules - this class only wires and sequences them.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputController _inputController;
        [SerializeField] private CombatHudView _combatHudView;
        [SerializeField] private RunFlowView _runFlowView;
        [SerializeField] private RelicRewardView _relicRewardView;

        [Header("Board size")]
        [SerializeField] private int _boardWidth = 8;
        [SerializeField] private int _boardHeight = 8;

        [Header("Run tuning")]
        [SerializeField] private int _roomCount = 4;
        [SerializeField] private int _baseMonsterHealth = 24;
        [SerializeField] private int _healthIncreasePerRoom = 10;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;
        [SerializeField] private int _relicOptionsPerReward = 2;

        private readonly System.Random _random = new System.Random();

        private Run _run;
        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;

        private RelicSet _relics;
        private List<IRelic> _relicPool;
        private IReadOnlyList<IRelic> _pendingRelicOptions;

        private bool _acceptingInput;

        private void Start()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _runFlowView.ContinuePressed += HandleContinuePressed;
            _runFlowView.RestartPressed += HandleRestartPressed;
            _relicRewardView.RelicChosen += HandleRelicChosen;

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

            if (_relicRewardView != null)
            {
                _relicRewardView.RelicChosen -= HandleRelicChosen;
            }

            UnsubscribeObjective();
        }

        private void StartNewRun()
        {
            _relics = new RelicSet();
            _relicPool = new List<IRelic>(RelicCatalog.CreateDefault());
            _run = new Run(BuildRooms());

            _runFlowView.HideRunEnd();
            _runFlowView.HideBetweenRooms();
            _relicRewardView.Hide();
            _runFlowView.ShowRelics(_relics.Relics);

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
            _objective = new MonsterCombatObjective(room.MonsterHealth, room.MoveLimit, room.DamagePerTile, _relics);
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
            _run.RegisterRoomResult(won: status == ObjectiveStatus.Won);

            switch (_run.Status)
            {
                case RunStatus.Won:
                    _runFlowView.ShowRunResult(won: true);
                    break;
                case RunStatus.Lost:
                    _runFlowView.ShowRunResult(won: false);
                    break;
                default:
                    OfferRelicRewardOrContinue();
                    break;
            }
        }

        private void OfferRelicRewardOrContinue()
        {
            _pendingRelicOptions = RelicRewardGenerator.PickOptions(_relicPool, _relicOptionsPerReward, _random);

            if (_pendingRelicOptions.Count == 0)
            {
                _runFlowView.ShowBetweenRooms();
                return;
            }

            _relicRewardView.Show(_pendingRelicOptions);
        }

        private void HandleRelicChosen(int optionIndex)
        {
            if (_pendingRelicOptions == null || optionIndex < 0 || optionIndex >= _pendingRelicOptions.Count)
            {
                return;
            }

            IRelic chosen = _pendingRelicOptions[optionIndex];
            _relics.Add(chosen);
            _relicPool.Remove(chosen);
            _pendingRelicOptions = null;

            _runFlowView.ShowRelics(_relics.Relics);
            _relicRewardView.Hide();
            LoadCurrentRoom();
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