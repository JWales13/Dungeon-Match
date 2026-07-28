using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Minimal single-floor player (Phase 0 clean base for the DungeonVision
    /// pivot). Builds one combat board, wires input and the HUD, and shows a
    /// win/lose result with a button to try a fresh floor. No runs, relics, or
    /// meta flow - those retired systems are being replaced by the new
    /// board/booster/station design.
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

        [Header("Floor tuning")]
        [SerializeField] private int _monsterHealth = 30;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;

        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;
        private bool _acceptingInput;

        private void Awake()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _runFlowView.RestartPressed += HandleRestartPressed;
        }

        private void Start()
        {
            LoadFloor();
        }

        private void OnDestroy()
        {
            if (_inputController != null)
            {
                _inputController.SwapRequested -= HandleSwapRequested;
            }

            if (_runFlowView != null)
            {
                _runFlowView.RestartPressed -= HandleRestartPressed;
            }

            UnsubscribeObjective();
        }

        private void LoadFloor()
        {
            UnsubscribeObjective();

            _board = new Board(_boardWidth, _boardHeight, new MatchFinder());
            _objective = new MonsterCombatObjective(_monsterHealth, _moveLimit, _damagePerTile);
            _objectiveDriver = new BoardObjectiveDriver(_board, _objective);
            _objective.StatusChanged += HandleObjectiveStatusChanged;

            _boardView.Initialize(_board);
            _combatHudView.Initialize(_objective);
            _runFlowView.HideRunEnd();

            _acceptingInput = true;
        }

        private void HandleObjectiveStatusChanged(ObjectiveStatus status)
        {
            if (status == ObjectiveStatus.InProgress)
            {
                return;
            }

            _acceptingInput = false;
            _runFlowView.ShowRunResult(won: status == ObjectiveStatus.Won);
        }

        private void HandleRestartPressed()
        {
            _runFlowView.HideRunEnd();
            LoadFloor();
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
                _objective.StatusChanged -= HandleObjectiveStatusChanged;
            }
        }
    }
}