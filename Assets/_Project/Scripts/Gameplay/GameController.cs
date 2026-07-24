using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Composition root for the playable scene. Builds the domain objects
    /// (Board, objective, driver), connects them to the presentation objects
    /// (BoardView, CombatHudView), and routes player input into board moves.
    /// Also stops accepting input once the encounter is won or lost.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputController _inputController;
        [SerializeField] private CombatHudView _combatHudView;

        [Header("Board size")]
        [SerializeField] private int _boardWidth = 8;
        [SerializeField] private int _boardHeight = 8;

        [Header("Encounter tuning")]
        [SerializeField] private int _monsterHealth = 30;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;

        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;
        private bool _acceptingInput = true;

        private void Start()
        {
            _board = new Board(_boardWidth, _boardHeight, new MatchFinder());
            _objective = new MonsterCombatObjective(_monsterHealth, _moveLimit, _damagePerTile);
            _objectiveDriver = new BoardObjectiveDriver(_board, _objective);

            _boardView.Initialize(_board);
            _combatHudView.Initialize(_objective);

            _inputController.SwapRequested += HandleSwapRequested;
            _objective.StatusChanged += HandleObjectiveStatusChanged;
        }

        private void OnDestroy()
        {
            if (_inputController != null)
            {
                _inputController.SwapRequested -= HandleSwapRequested;
            }

            if (_objective != null)
            {
                _objective.StatusChanged -= HandleObjectiveStatusChanged;
            }
        }

        private void HandleSwapRequested(Vector2Int a, Vector2Int b)
        {
            if (!_acceptingInput)
            {
                return;
            }

            _board.TrySwap(a, b);
        }

        private void HandleObjectiveStatusChanged(ObjectiveStatus status)
        {
            if (status != ObjectiveStatus.InProgress)
            {
                _acceptingInput = false;
            }
        }
    }
}