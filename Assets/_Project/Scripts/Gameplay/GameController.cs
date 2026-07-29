using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The Floor scene's controller: plays one combat floor. Loads the
    /// stashes, wallet, and tower depth; asks FloorDifficultyCurve for that
    /// depth's FloorSpec (board size, monster HP, move limit, Gold reward);
    /// runs the board and the in-floor Dynamite; tallies ingredients
    /// harvested; and on a win pays out Gold + a Prize Voucher and advances
    /// depth one floor deeper. On floor end it saves and shows the result;
    /// Exit returns to the Green Room. Crafting (the Bomb Bench et al.) lives
    /// in the Green Room.
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
        [SerializeField] private BoosterLoadoutView _loadoutView;

        [Header("Difficulty curve (depth-driven - see FloorDifficultyCurve)")]
        [SerializeField] private int _baseMonsterHealth = 30;
        [SerializeField] private int _monsterHealthPerDepth = 6;
        [SerializeField] private int _baseMoveLimit = 15;
        [SerializeField] private int _moveLimitPerDepth = 1;
        [SerializeField] private int _maxMoveLimit = 20;
        [Tooltip("Board size growth is 0 by default - BoardView doesn't yet re-fit the camera to a " +
                 "changing board, so a non-zero value here will run tiles off-screen. Leave at 0 until " +
                 "that view work lands.")]
        [SerializeField] private int _baseBoardSize = 8;
        [SerializeField] private int _boardSizePerDepth = 0;
        [SerializeField] private int _maxBoardSize = 8;
        [SerializeField] private int _baseGoldReward = 25;
        [SerializeField] private int _goldRewardPerDepth = 5;

        [Header("Floor tuning")]
        [SerializeField] private int _damagePerTile = 1;
        [SerializeField] private int _ingredientsPerDetonation = 2;

        [Header("Win rewards")]
        [SerializeField] private int _prizeVouchersPerWin = 1;

        [Header("Continue (on fail)")]
        [SerializeField] private int _continueBaseCost = 20;
        [SerializeField] private int _continueCostStep = 20;
        [SerializeField] private int _extraMovesPerContinue = 5;

        private IngredientInventoryRepository _ingredientRepository;
        private IngredientInventory _inventory;
        private BoosterInventoryRepository _boosterRepository;
        private BoosterInventory _boosters;
        private WalletRepository _walletRepository;
        private Wallet _wallet;
        private TowerProgressRepository _towerProgressRepository;
        private TowerProgress _towerProgress;
        private FloorDifficultyCurve _difficultyCurve;
        private FloorSpec _currentFloorSpec;

        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;
        private IngredientHarvester _harvester;
        private readonly Dictionary<TileType, int> _floorHarvest = new Dictionary<TileType, int>();
        private bool _acceptingInput;

        private bool _dynamiteArmed;
        private bool _dynamiteUsedThisFloor;
        private int _continuesUsedThisFloor;

        private void Awake()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _inputController.CellTapped += HandleCellTapped;
            _floorResultView.ExitPressed += HandleExit;
            _floorResultView.ContinuePressed += HandleContinuePressed;
            _floorResultView.RetryPressed += HandleRetryPressed;
            _loadoutView.UseDynamitePressed += HandleUseDynamitePressed;
        }

        private void Start()
        {
            _ingredientRepository = new IngredientInventoryRepository();
            _inventory = _ingredientRepository.Load();
            _boosterRepository = new BoosterInventoryRepository();
            _boosters = _boosterRepository.Load();
            _boosters.Changed += RefreshLoadout;
            _walletRepository = new WalletRepository();
            _wallet = _walletRepository.Load();
            _towerProgressRepository = new TowerProgressRepository();
            _towerProgress = _towerProgressRepository.Load();

            _difficultyCurve = new FloorDifficultyCurve(
                _baseMonsterHealth, _monsterHealthPerDepth,
                _baseMoveLimit, _moveLimitPerDepth, _maxMoveLimit,
                _baseBoardSize, _boardSizePerDepth, _maxBoardSize,
                _baseGoldReward, _goldRewardPerDepth);

            _ingredientHudView.Initialize(_inventory);

            LoadFloor();
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
                _floorResultView.ExitPressed -= HandleExit;
                _floorResultView.ContinuePressed -= HandleContinuePressed;
                _floorResultView.RetryPressed -= HandleRetryPressed;
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

            _currentFloorSpec = _difficultyCurve.Generate(_towerProgress.CurrentDepth);

            _board = new Board(_currentFloorSpec.BoardSize, _currentFloorSpec.BoardSize, new MatchFinder());
            _objective = new MonsterCombatObjective(_currentFloorSpec.MonsterHealth, _currentFloorSpec.MoveLimit, _damagePerTile);
            _objectiveDriver = new BoardObjectiveDriver(_board, _objective);
            _harvester = new IngredientHarvester(_board, _inventory, _ingredientsPerDetonation);
            _objective.StatusChanged += HandleObjectiveStatusChanged;
            _board.PowerTileDetonated += HandleHarvestTally;

            _floorHarvest.Clear();

            _boardView.Initialize(_board);
            _combatHudView.Initialize(_objective);
            _combatHudView.SetDepth(_currentFloorSpec.Depth);
            _floorResultView.HideResult();

            _continuesUsedThisFloor = 0;
            _dynamiteArmed = false;
            _dynamiteUsedThisFloor = false;
            _acceptingInput = true;
            RefreshLoadout();
        }

        private void HandleHarvestTally(TileType color)
        {
            _floorHarvest.TryGetValue(color, out int current);
            _floorHarvest[color] = current + _ingredientsPerDetonation;
        }

        private void HandleObjectiveStatusChanged(ObjectiveStatus status)
        {
            if (status == ObjectiveStatus.InProgress)
            {
                return;
            }

            _acceptingInput = false;
            _dynamiteArmed = false;
            RefreshLoadout();

            if (status == ObjectiveStatus.Won)
            {
                int goldEarned = _currentFloorSpec.GoldReward;
                _wallet.Add(CurrencyType.Gold, goldEarned);
                _wallet.Add(CurrencyType.PrizeVoucher, _prizeVouchersPerWin);
                _towerProgress.AdvanceDepth(); // next Play/Descend starts one floor deeper
                SaveAll();
                _floorResultView.ShowWin(goldEarned, _prizeVouchersPerWin, BuildHarvestSummary());
            }
            else
            {
                SaveAll();
                int continueCost = CurrentContinueCost();
                bool canAfford = _wallet.GetBalance(CurrencyType.Gold) >= continueCost;
                _floorResultView.ShowFail(continueCost, canAfford);
            }
        }

        private string BuildHarvestSummary()
        {
            if (_floorHarvest.Count == 0)
            {
                return string.Empty;
            }

            ITheme theme = Theme.Current;
            var parts = new List<string>();
            foreach (KeyValuePair<TileType, int> entry in _floorHarvest)
            {
                parts.Add($"{theme.GetIngredientName(entry.Key)} x{entry.Value}");
            }

            return string.Join(", ", parts);
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

        private void HandleExit()
        {
            SceneManager.LoadScene(SceneNames.GreenRoom);
        }

        private void HandleContinuePressed()
        {
            int cost = CurrentContinueCost();
            if (!_wallet.TrySpend(CurrencyType.Gold, cost))
            {
                return;
            }

            _continuesUsedThisFloor++;
            _walletRepository.Save(_wallet);
            _objective.Continue(_extraMovesPerContinue); // revives the same board with +moves

            _floorResultView.HideResult();
            _acceptingInput = true;
            RefreshLoadout();
        }

        private void HandleRetryPressed()
        {
            LoadFloor(); // fresh board, same difficulty, free
        }

        private int CurrentContinueCost()
        {
            return _continueBaseCost + (_continueCostStep * _continuesUsedThisFloor);
        }

        private void SaveAll()
        {
            _ingredientRepository.Save(_inventory);
            _boosterRepository.Save(_boosters);
            _walletRepository.Save(_wallet);
            _towerProgressRepository.Save(_towerProgress);
        }

        private void UnsubscribeObjective()
        {
            if (_objective != null)
            {
                _objective.StatusChanged -= HandleObjectiveStatusChanged;
            }

            if (_board != null)
            {
                _board.PowerTileDetonated -= HandleHarvestTally;
            }
        }
    }
}