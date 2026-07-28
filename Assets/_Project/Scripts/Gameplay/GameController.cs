using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Single-floor player plus the (temporary in-combat) Bomb Bench. Loads the
    /// ingredient and booster stashes once, ticks the station each frame,
    /// harvests ingredients from detonations, and saves both stashes at floor
    /// end (win or lose). Collecting from the bench banks Dynamite immediately.
    ///
    /// The station lives on the combat screen only because there's no Green Room
    /// yet (Phase 5); it will move to the hub then.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputController _inputController;
        [SerializeField] private CombatHudView _combatHudView;
        [SerializeField] private RunFlowView _runFlowView;
        [SerializeField] private IngredientHudView _ingredientHudView;
        [SerializeField] private StationView _stationView;

        [Header("Board size")]
        [SerializeField] private int _boardWidth = 8;
        [SerializeField] private int _boardHeight = 8;

        [Header("Floor tuning")]
        [SerializeField] private int _monsterHealth = 30;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;
        [SerializeField] private int _ingredientsPerDetonation = 2;

        [Header("Bomb Bench")]
        [SerializeField] private int _bombBenchIngredientCost = 3;
        [SerializeField] private float _bombBenchProductionSeconds = 10f;
        [SerializeField] private int _bombBenchBufferCapacity = 2;

        private IngredientInventoryRepository _ingredientRepository;
        private IngredientInventory _inventory;
        private BoosterInventoryRepository _boosterRepository;
        private BoosterInventory _boosters;
        private ProducerStation _bombBench;

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
            _ingredientRepository = new IngredientInventoryRepository();
            _inventory = _ingredientRepository.Load();
            _boosterRepository = new BoosterInventoryRepository();
            _boosters = _boosterRepository.Load();

            _bombBench = new ProducerStation(
                BoosterType.Dynamite, TileType.Red,
                _bombBenchIngredientCost, _bombBenchProductionSeconds, _bombBenchBufferCapacity, _inventory);

            _ingredientHudView.Initialize(_inventory);
            _stationView.Initialize(_bombBench, _boosters);

            LoadFloor();
        }

        private void Update()
        {
            if (_bombBench == null)
            {
                return;
            }

            _bombBench.Tick(Time.deltaTime);
            AutoCollect();
        }

        /// <summary>Boosters deposit into the stash automatically as they finish - no manual collect.</summary>
        private void AutoCollect()
        {
            int collected = _bombBench.Collect();
            if (collected > 0)
            {
                _boosters.Add(BoosterType.Dynamite, collected);
                _boosterRepository.Save(_boosters);
            }
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
            SaveStashes();
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

        private void SaveStashes()
        {
            _ingredientRepository.Save(_inventory);
            _boosterRepository.Save(_boosters);
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