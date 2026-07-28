using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Single-floor player. Builds one combat board, wires input, the combat
    /// HUD, and ingredient harvesting, and shows a win/lose result with a button
    /// to try a fresh floor. The ingredient stash is loaded once and saved at
    /// the end of each floor (win or lose), so harvested ingredients always
    /// bank.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputController _inputController;
        [SerializeField] private CombatHudView _combatHudView;
        [SerializeField] private RunFlowView _runFlowView;
        [SerializeField] private IngredientHudView _ingredientHudView;

        [Header("Board size")]
        [SerializeField] private int _boardWidth = 8;
        [SerializeField] private int _boardHeight = 8;

        [Header("Floor tuning")]
        [SerializeField] private int _monsterHealth = 30;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;
        [SerializeField] private int _ingredientsPerDetonation = 2;

        private IngredientInventoryRepository _inventoryRepository;
        private IngredientInventory _inventory;

        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;
        private IngredientHarvester _harvester;
        private bool _acceptingInput;

        private void Awake()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _runFlowView.RestartPressed += HandleRestartPressed;
        }

        private void Start()
        {
            _inventoryRepository = new IngredientInventoryRepository();
            _inventory = _inventoryRepository.Load();
            _ingredientHudView.Initialize(_inventory);

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
            _harvester = new IngredientHarvester(_board, _inventory, _ingredientsPerDetonation);
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
            _inventoryRepository.Save(_inventory); // bank harvested ingredients, win or lose
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