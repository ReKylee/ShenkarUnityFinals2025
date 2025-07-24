using Core.Data;
using Core.Events;
using Core.Services;
using UnityEngine;
using VContainer;

namespace Core
{
    /// <summary>
    ///     Coordinates between game events and data storage.
    ///     Responsible only for translating events into data updates and managing save operations.
    /// </summary>
    public class GameDataCoordinator : MonoBehaviour
    {
        [SerializeField] private float autoSaveInterval = 30;
        private IAutoSaveService _autoSaveService;
        private IEventBus _eventBus;
        private IGameDataService _gameDataService;
        private bool _isInitialized;

        #region VContainer Injection

        [Inject]
        public void Construct(
            IGameDataService gameDataService,
            IEventBus eventBus,
            IAutoSaveService autoSaveService)
        {
            _gameDataService = gameDataService;
            _eventBus = eventBus;
            _autoSaveService = autoSaveService;
            _isInitialized = true;


            // Initialize after dependencies are injected
            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // Subscribe to events - only subscribe to events that affect game data
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted); // For best time tracking
            _eventBus?.Subscribe<LevelStartedEvent>(OnLevelStarted); // For current level tracking
            _eventBus?.Subscribe<PlayerLivesChangedEvent>(OnLivesChanged); // For lives tracking
            _eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged); // For game state changes

            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDied);

            if (_gameDataService != null)
                _gameDataService.OnDataChanged += OnGameDataChanged;

            if (_autoSaveService != null)
            {
                _autoSaveService.OnSaveRequested += SaveData;
                _autoSaveService.SaveInterval = autoSaveInterval;
                _autoSaveService.IsEnabled = true;
            }
        }

        #endregion

        #region Private Methods

        private void SaveData()
        {
            _gameDataService?.SaveData();
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // If not yet initialized (dependencies not injected), wait
            if (!_isInitialized)
            {
            }
        }

        private void Update()
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.Update();
        }

        private void OnDestroy()
        {
            if (!_isInitialized) return;

            // Unsubscribe from events
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<PlayerDeathEvent>(OnPlayerDied);
                _eventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
                _eventBus.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
                _eventBus.Unsubscribe<PlayerLivesChangedEvent>(OnLivesChanged);
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            }


            if (_gameDataService != null)
                _gameDataService.OnDataChanged -= OnGameDataChanged;

            if (_autoSaveService != null)
            {
                _autoSaveService.OnSaveRequested -= SaveData;
                _autoSaveService.ForceSave();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.OnApplicationFocus(hasFocus);
        }

        #endregion

        #region Event Handlers

        private void OnPlayerDied(PlayerDeathEvent deathEvent)
        {
            _autoSaveService?.RequestSave();
        }

        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            _gameDataService?.UpdateBestTime(levelEvent.CompletionTime);
            _autoSaveService?.RequestSave();
        }

        private void OnGameStateChanged(GameStateChangedEvent stateEvent)
        {
            // Handle data operations based on state changes
            if (stateEvent.NewState == GameState.Victory || stateEvent.NewState == GameState.GameOver)
            {
                _autoSaveService?.ForceSave();
            }
        }
        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            _gameDataService?.UpdateCurrentLevel(levelEvent.LevelName);
        }
        private void OnLivesChanged(PlayerLivesChangedEvent livesEvent)
        {
            Debug.Log(
                $"[GameDataCoordinator] Received PlayerLivesChangedEvent: CurrentLives={livesEvent.CurrentLives}");

            _gameDataService?.UpdateLives(livesEvent.CurrentLives);
            Debug.Log($"[GameDataCoordinator] Updated GameData lives to: {_gameDataService?.CurrentData.lives}");
        }

        private void OnGameDataChanged(GameData newData)
        {
            _autoSaveService?.RequestSave();
        }

        #endregion

        #region Public API

        public GameData GetCurrentData()
        {
            if (!_isInitialized || _gameDataService == null) return null;

            return _gameDataService.CurrentData;
        }

        public void ResetAllData()
        {
            if (!_isInitialized) return;

            _gameDataService?.ResetAllData();
        }

        #endregion

    }
}
