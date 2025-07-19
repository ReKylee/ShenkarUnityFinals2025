using Core.Data;
using Core.Events;
using Core.Flow;
using Core.Services;
using UnityEngine;
using VContainer;

namespace Core
{
    public class GameDataCoordinator : MonoBehaviour
    {
        private IGameDataService _gameDataService;
        private IEventBus _eventBus;
        private IAutoSaveService _autoSaveService;
        private IGameFlowController _gameFlowController;
        private bool _isInitialized = false;

        #region VContainer Injection

        [Inject]
        public void Construct(
            IGameDataService gameDataService,
            IEventBus eventBus,
            IAutoSaveService autoSaveService,
            IGameFlowController gameFlowController)
        {
            _gameDataService = gameDataService;
            _eventBus = eventBus;
            _autoSaveService = autoSaveService;
            _gameFlowController = gameFlowController;
            _isInitialized = true;

            // Initialize after dependencies are injected
            Initialize();
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // If not yet initialized (dependencies not injected), wait
            if (!_isInitialized)
            {
                return;
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
                _eventBus.Unsubscribe<PlayerLivesChangedEvent>(OnLivesChanged);
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

        #region Initialization

        private void Initialize()
        {
            // Subscribe to events
            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus?.Subscribe<PlayerLivesChangedEvent>(OnLivesChanged);


            if (_gameDataService != null)
                _gameDataService.OnDataChanged += OnGameDataChanged;

            if (_autoSaveService != null)
                _autoSaveService.OnSaveRequested += SaveData;

        }

        #endregion

        #region Event Handlers

        private void OnPlayerDied(PlayerDeathEvent deathEvent)
        {
            _gameFlowController?.HandlePlayerDeath();
            _autoSaveService?.RequestSave();
        }

        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            _gameDataService?.UpdateBestTime(levelEvent.CompletionTime);
            _autoSaveService?.RequestSave();
        }
        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            _gameDataService?.UpdateCurrentLevel(levelEvent.LevelName);
        }
        private void OnLivesChanged(PlayerLivesChangedEvent livesEvent)
        {
            Debug.Log($"[GameDataCoordinator] Received PlayerLivesChangedEvent: CurrentLives={livesEvent.CurrentLives}");
            _gameDataService?.UpdateLives(livesEvent.CurrentLives);
            Debug.Log($"[GameDataCoordinator] Updated GameData lives to: {_gameDataService?.CurrentData.lives}");
        }

        private void OnGameDataChanged(GameData newData)
        {
            _autoSaveService?.RequestSave();
        }

        #endregion

        #region Public API

        public void AddScore(int points)
        {
            if (!_isInitialized || _gameDataService == null) return;

            int newScore = _gameDataService.CurrentData.score + points;
            _gameDataService.UpdateScore(newScore);
        }

        public void AddCoins(int coinCount)
        {
            if (!_isInitialized || _gameDataService == null) return;

            int newCoins = _gameDataService.CurrentData.coins + coinCount;
            _gameDataService.UpdateCoins(newCoins);
        }

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

        #region Private Methods

        private void SaveData()
        {
            _gameDataService?.SaveData();
        }

        #endregion

    }
}
