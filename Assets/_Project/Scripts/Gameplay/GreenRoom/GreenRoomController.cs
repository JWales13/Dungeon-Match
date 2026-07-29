using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// The Green Room (hub) scene's controller. Owns the crafting side of the
    /// game: for every station in the Station Catalog it loads (or defaults)
    /// a build/upgrade StationProgress, runs the ones that are built (ticking
    /// production, auto-collecting into the booster stash), and wires each to
    /// its StationPanelView so Build/Upgrade button presses spend Prize
    /// Vouchers. The Play button saves and loads a floor.
    ///
    /// Production runs in real time while you're in the hub, plus a one-shot
    /// catch-up at Start for however long the app was closed (see
    /// ApplyOfflineCatchUp / ProducerStation.FastForward), capped at
    /// _maxOfflineSeconds so a long absence can't bank an unbounded backlog.
    /// </summary>
    public class GreenRoomController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Button _playButton;
        [SerializeField] private IngredientHudView _ingredientHudView;
        [SerializeField] private CurrencyHudView _currencyHudView;

        [Header("Stations")]
        [Tooltip("The Station Catalog asset holding every Producer station's tuning.")]
        [SerializeField] private StationCatalogAsset _catalog;
        [Tooltip("One panel per catalog station. Each panel's Station Output must match a catalog entry.")]
        [SerializeField] private List<StationPanelView> _stationPanels;

        [Header("Offline catch-up")]
        [Tooltip("Caps how much real-world offline time counts toward production, so a long absence doesn't bank an unbounded backlog beyond this window.")]
        [SerializeField] private float _maxOfflineSeconds = 8 * 60 * 60f; // 8 hours

        private IngredientInventoryRepository _ingredientRepository;
        private IngredientInventory _inventory;
        private BoosterInventoryRepository _boosterRepository;
        private BoosterInventory _boosters;
        private WalletRepository _walletRepository;
        private Wallet _wallet;
        private StationProgressRepository _stationProgressRepository;
        private OfflineClockRepository _offlineClockRepository;

        private readonly List<StationRuntime> _stationRuntimes = new List<StationRuntime>();

        /// <summary>One catalog station's live state: its build progress, the ProducerStation it runs once built (null until then), and the panel showing it.</summary>
        private class StationRuntime
        {
            public StationDefinition Definition;
            public StationProgress Progress;
            public ProducerStation Producer;
            public StationPanelView Panel;
            public Action AdvanceHandler;
        }

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

            foreach (StationRuntime runtime in _stationRuntimes)
            {
                if (runtime.Panel != null)
                {
                    runtime.Panel.AdvancePressed -= runtime.AdvanceHandler;
                }
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
            _stationProgressRepository = new StationProgressRepository();
            _offlineClockRepository = new OfflineClockRepository();

            _ingredientHudView.Initialize(_inventory);
            _currencyHudView.Initialize(_wallet);

            BuildStations();
            ApplyOfflineCatchUp();
        }

        /// <summary>
        /// Fast-forwards every built station's producer by however long the
        /// Green Room was last closed, capped at _maxOfflineSeconds, then
        /// stamps the clock so the next launch measures from now. Boosters
        /// completed this way land in each producer's buffer same as always -
        /// the normal Update loop collects them into the stash next frame.
        /// </summary>
        private void ApplyOfflineCatchUp()
        {
            DateTime? lastSaved = _offlineClockRepository.Load();
            DateTime now = DateTime.UtcNow;

            if (lastSaved.HasValue)
            {
                float elapsedSeconds = Mathf.Clamp((float)(now - lastSaved.Value).TotalSeconds, 0f, _maxOfflineSeconds);
                if (elapsedSeconds > 0f)
                {
                    foreach (StationRuntime runtime in _stationRuntimes)
                    {
                        runtime.Producer?.FastForward(elapsedSeconds);
                    }
                }
            }

            _offlineClockRepository.Save(now);
        }

        private void BuildStations()
        {
            if (_catalog == null)
            {
                Debug.LogWarning("GreenRoomController has no Station Catalog assigned - no stations will run.");
                return;
            }

            IReadOnlyDictionary<BoosterType, int> savedLevels = _stationProgressRepository.LoadLevels();

            foreach (StationTuning tuning in _catalog.stations)
            {
                StationPanelView panel = FindPanelFor(tuning.output);
                if (panel == null)
                {
                    Debug.LogWarning($"No StationPanelView wired for {tuning.output} - check GreenRoomController's Station Panels list.");
                    continue;
                }

                StationDefinition definition = tuning.BuildDefinition();
                int initialLevel = ResolveInitialLevel(tuning, savedLevels);
                var progress = new StationProgress(definition, _wallet, initialLevel);

                var runtime = new StationRuntime { Definition = definition, Progress = progress, Panel = panel };
                runtime.AdvanceHandler = () => HandleAdvancePressed(runtime);
                panel.AdvancePressed += runtime.AdvanceHandler;

                panel.Initialize(tuning.displayName, progress, _boosters, _wallet);
                RebuildProducer(runtime);

                _stationRuntimes.Add(runtime);
            }
        }

        private static int ResolveInitialLevel(StationTuning tuning, IReadOnlyDictionary<BoosterType, int> savedLevels)
        {
            if (savedLevels.TryGetValue(tuning.output, out int savedLevel))
            {
                return savedLevel;
            }

            // No save entry for this station (first run, or a station added
            // after the player's last save) - fall back to the catalog default.
            return tuning.startsBuilt ? 1 : 0;
        }

        private StationPanelView FindPanelFor(BoosterType output)
        {
            foreach (StationPanelView panel in _stationPanels)
            {
                if (panel != null && panel.StationOutput == output)
                {
                    return panel;
                }
            }

            return null;
        }

        private void Update()
        {
            foreach (StationRuntime runtime in _stationRuntimes)
            {
                if (runtime.Producer == null)
                {
                    continue;
                }

                runtime.Producer.Tick(Time.deltaTime);
                AutoCollect(runtime);
            }
        }

        private void AutoCollect(StationRuntime runtime)
        {
            int collected = runtime.Producer.Collect();
            if (collected > 0)
            {
                _boosters.Add(runtime.Definition.Output, collected);
                SaveAll();
            }
        }

        private void HandleAdvancePressed(StationRuntime runtime)
        {
            if (!runtime.Progress.TryAdvance())
            {
                return;
            }

            RebuildProducer(runtime);
            SaveAll();
        }

        /// <summary>
        /// (Re)creates the ProducerStation for a station's current level,
        /// carrying over any uncollected boosters from the previous instance
        /// (clamped to the new buffer capacity) so upgrading never loses stock.
        /// </summary>
        private void RebuildProducer(StationRuntime runtime)
        {
            if (!runtime.Progress.IsBuilt)
            {
                runtime.Producer = null;
                runtime.Panel.SetProducer(null);
                return;
            }

            StationLevelConfig config = runtime.Progress.CurrentConfig.Value;
            int carriedBuffer = runtime.Producer != null ? runtime.Producer.BufferCount : 0;
            int seedBuffer = Mathf.Min(carriedBuffer, config.BufferCapacity);

            runtime.Producer = new ProducerStation(
                runtime.Definition.Output, runtime.Definition.IngredientColor,
                config.IngredientCost, config.ProductionSeconds, config.BufferCapacity,
                _inventory, seedBuffer);

            runtime.Panel.SetProducer(runtime.Producer);
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
            SaveStationProgress();
            _offlineClockRepository.Save(DateTime.UtcNow);
        }

        private void SaveStationProgress()
        {
            var progresses = new List<StationProgress>(_stationRuntimes.Count);
            foreach (StationRuntime runtime in _stationRuntimes)
            {
                progresses.Add(runtime.Progress);
            }

            _stationProgressRepository.Save(progresses);
        }
    }
}
