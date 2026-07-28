using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// The Green Room (hub) scene's controller. Owns the crafting side of the
    /// game: it loads the stashes and wallet, runs the Bomb Bench (which brews
    /// Dynamite from Gunpowder over time and auto-deposits it), and shows the
    /// player's stocks and currencies. The Play button saves and loads a floor.
    ///
    /// Production runs in real time while you're in the hub. Offline catch-up
    /// (producing while the app is closed) comes in 5d.
    /// </summary>
    public class GreenRoomController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Button _playButton;
        [SerializeField] private StationView _stationView;
        [SerializeField] private IngredientHudView _ingredientHudView;
        [SerializeField] private CurrencyHudView _currencyHudView;

        [Header("Bomb Bench")]
        [SerializeField] private int _bombBenchIngredientCost = 3;
        [SerializeField] private float _bombBenchProductionSeconds = 10f;
        [SerializeField] private int _bombBenchBufferCapacity = 2;

        private IngredientInventoryRepository _ingredientRepository;
        private IngredientInventory _inventory;
        private BoosterInventoryRepository _boosterRepository;
        private BoosterInventory _boosters;
        private WalletRepository _walletRepository;
        private Wallet _wallet;
        private ProducerStation _bombBench;

        private void Awake()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(Play);
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(Play);
            }
        }

        private void Start()
        {
            _ingredientRepository = new IngredientInventoryRepository();
            _inventory = _ingredientRepository.Load();
            _boosterRepository = new BoosterInventoryRepository();
            _boosters = _boosterRepository.Load();
            _walletRepository = new WalletRepository();
            _wallet = _walletRepository.Load();

            _bombBench = new ProducerStation(
                BoosterType.Dynamite, TileType.Red,
                _bombBenchIngredientCost, _bombBenchProductionSeconds, _bombBenchBufferCapacity, _inventory);

            _stationView.Initialize(_bombBench, _boosters);
            _ingredientHudView.Initialize(_inventory);
            _currencyHudView.Initialize(_wallet);
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

        private void AutoCollect()
        {
            int collected = _bombBench.Collect();
            if (collected > 0)
            {
                _boosters.Add(BoosterType.Dynamite, collected);
                SaveAll();
            }
        }

        private void Play()
        {
            SaveAll();
            SceneManager.LoadScene(SceneNames.Floor);
        }

        private void SaveAll()
        {
            _ingredientRepository.Save(_inventory);
            _boosterRepository.Save(_boosters);
            _walletRepository.Save(_wallet);
        }
    }
}