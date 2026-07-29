using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The Floor scene's controller: plays one combat floor. Loads the stashes
    /// and wallet, runs the board and the in-floor Dynamite, tallies the
    /// ingredients harvested this floor, and on a win pays out Gold + a Prize
    /// Voucher. On floor end it saves and shows the result; Exit returns to the
    /// Green Room. Crafting (the Bomb Bench) lives in the Green Room.
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

        [Header("Board size")]
        [SerializeField] private int _boardWidth = 8;
        [SerializeField] private int _boardHeight = 8;

        [Header("Floor tuning")]
        [SerializeField] private int _monsterHealth = 30;
        [SerializeField] private int _moveLimit = 15;
        [SerializeField] private int _damagePerTile = 1;
        [SerializeField] private int _ingredientsPerDetonation = 2;

        [Header("Win rewards")]
        [SerializeField] private int _goldPerWin = 25;
        [SerializeField] private int _prizeVouchersPerWin = 1;

        private IngredientInventoryRepository _ingredientRepository;
        private IngredientInventory _inventory;
        private BoosterInventoryRepository _boosterRepository;
        private BoosterInventory _boosters;
        private WalletRepository _walletRepository;
        private Wallet _wallet;

        private Board _board;
        private MonsterCombatObjective _objective;
        private BoardObjectiveDriver _objectiveDriver;
        private IngredientHarvester _harvester;
        private readonly Dictionary<TileType, int> _floorHarvest = new Dictionary<TileType, int>();
        private bool _acceptingInput;

        private bool _dynamiteArmed;
        private bool _dynamiteUsedThisFloor;

        private void Awake()
        {
            _inputController.SwapRequested += HandleSwapRequested;
            _inputController.CellTapped += HandleCellTapped;
            _floorResultView.ExitPressed += HandleExit;
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
            _board.PowerTileDetonated += HandleHarvestTally;

            _floorHarvest.Clear();

            _boardView.Initialize(_board);
            _combatHudView.Initialize(_objective);
            _floorResultView.HideResult();

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
                _wallet.Add(CurrencyType.Gold, _goldPerWin);
                _wallet.Add(CurrencyType.PrizeVoucher, _prizeVouchersPerWin);
                SaveAll();
                _floorResultView.ShowWin(_goldPerWin, _prizeVouchersPerWin, BuildHarvestSummary());
            }
            else
            {
                SaveAll();
                _floorResultView.ShowFail();
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

        private void SaveAll()
        {
            _ingredientRepository.Save(_inventory);
            _boosterRepository.Save(_boosters);
            _walletRepository.Save(_wallet);
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