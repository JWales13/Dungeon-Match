using UnityEngine;
using UnityEngine.Serialization;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Single-floor player plus the (temporary in-combat) Bomb Bench and the
    /// booster loadout. Loads the ingredient and booster stashes once, ticks the
    /// station each frame, harvests ingredients from detonations, and saves both
    /// stashes at floor end. Boosters auto-deposit as they finish; the player
    /// can bring one Dynamite into a floor and tap a tile to set it off.
    ///
    /// The station/loadout live on the combat screen only because there's no
    /// Green Room yet (Phase 5).
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputController _inputController;
        [SerializeField] private CombatHudView _combatHudView;

        [FormerlySerializedAs("_runFlowView")]
        [SerializeField] private FloorResultView _floorResultView;

        [SerializeField] private IngredientHudView _ingredientHudView;
        [SerializeField] private StationView _stationView;
        [SerializeField] private BoosterLoadoutView _loadoutView;

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

        private bool _dynamiteArmed;
        private bool _dynamiteUsedThisFloor;

        private void Awake()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _inputController.CellTapped += HandleCellTapped;
            _floorResultView.PlayAgainPressed += HandlePlayAgain;
            _loadoutView.UseDynamitePressed += HandleUseDynamitePressed;
        }

        private void Start()
        {
            _ingredientRepository = new IngredientInventoryRepository();
            _inventory = _ingredientRepository.Load();
            _boosterRepository = new BoosterInventoryRepository();
            _boosters = _boosterRepository.Load();
            _boosters.Changed += RefreshLoadout;

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
                _inputController.CellTapped -= HandleCellTapped;
            }

            if (_floorResultView != null)
            {
                _floorResultView.PlayAgainPressed -= HandlePlayAgain;
            }

            if (_loadoutView != null)
            {
                _loadoutView.UseDynamitePressed -= HandleUseDynamitePressed;
            }

            if (_boosters != null)
            {
                _boosters.Changed -= RefreshLoadout;
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
            _floorResultView.HideResult();

            _dynamiteArmed = false;
            _dynamiteUsedThisFloor = false;
            _acceptingInput = true;
            RefreshLoadout();
        }

        private void HandleObjectiveStatusChanged(ObjectiveStatus status)
        {
            if (status == ObjectiveStatus.InProgress)
            {
                return;
            }

            _acceptingInput = false;
            _dynamiteArmed = false;
            SaveStashes();
            RefreshLoadout();
            _floorResultView.ShowResult(won: status == ObjectiveStatus.Won);
        }

        private void HandleSwapRequested(Vector2Int a, Vector2Int b)
        {
            if (!_acceptingInput)
            {
                return;
            }

            // A swap that "takes" (forms a match, or moves a power tile) is one
            // move. Damage from the resulting clears is applied by the driver.
            if (_board.TrySwap(a, b))
            {
                _objective.SpendMove();
            }
        }

        private void HandleUseDynamitePressed()
        {
            if (!CanUseDynamite())
            {
                return;
            }

            _dynamiteArmed = true;
            RefreshLoadout();
        }

        private void HandleCellTapped(Vector2Int cell)
        {
            if (!_dynamiteArmed || !_acceptingInput)
            {
                return;
            }

            _board.UseAreaBlast(cell); // deals damage via the driver; costs no move
            _boosters.TrySpend(BoosterType.Dynamite, 1);
            _boosterRepository.Save(_boosters);

            _dynamiteArmed = false;
            _dynamiteUsedThisFloor = true;
            RefreshLoadout();
        }

        private bool CanUseDynamite()
        {
            return _acceptingInput && !_dynamiteUsedThisFloor && _boosters.GetCount(BoosterType.Dynamite) > 0;
        }

        private void RefreshLoadout()
        {
            _loadoutView.SetDynamite(_boosters.GetCount(BoosterType.Dynamite), CanUseDynamite(), _dynamiteArmed);
        }

        private void HandlePlayAgain()
        {
            _floorResultView.HideResult();
            LoadFloor();
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