using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// The Floor scene's controller: plays one combat floor. Loads the stashes
    /// (harvest adds ingredients; the loadout spends boosters), runs the board
    /// and the in-floor Dynamite, and on floor end saves and returns to the
    /// Green Room. Crafting (the Bomb Bench) lives in the Green Room now.
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

        private IngredientInventoryRepository _ingredientRepository;
        private IngredientInventory _inventory;
        private BoosterInventoryRepository _boosterRepository;
        private BoosterInventory _boosters;

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
            // Floor over -> back to the Green Room hub. Stashes were already
            // saved in HandleObjectiveStatusChanged, so state carries over.
            SceneManager.LoadScene(SceneNames.GreenRoom);
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