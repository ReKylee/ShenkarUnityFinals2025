using Core.Data;
using Core.Events;
using Core.Flow;
using Core.Lives;
using Core.Services;
using UnityEngine;
using VContainer;

namespace Core
{
    public class GameDataCoordinator : MonoBehaviour
    {
        private IGameDataService _gameDataService;
        private ILivesService _livesService;
        private IEventBus _eventBus;
        private IAutoSaveService _autoSaveService;
        private IGameFlowController _gameFlowController;
        private bool _isInitialized = false;

        #region VContainer Injection
        [Inject]
        public void Construct(
            IGameDataService gameDataService, 
            ILivesService livesService, 
            IEventBus eventBus,
            IAutoSaveService autoSaveService,
            IGameFlowController gameFlowController)
        {
            _gameDataService = gameDataService;
            _livesService = livesService;
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
            }
            
            if (_livesService != null)
                _livesService.OnLivesChanged -= OnLivesChanged;
            
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
            
            // Connect services
            if (_livesService != null)
                _livesService.OnLivesChanged += OnLivesChanged;
            
            if (_gameDataService != null)
                _gameDataService.OnDataChanged += OnGameDataChanged;
            
            if (_autoSaveService != null)
                _autoSaveService.OnSaveRequested += SaveData;
            
            // Initialize lives from saved data
            if (_livesService != null && _gameDataService != null)
                _livesService.SetLives(_gameDataService.CurrentData.lives);
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

        private void OnLivesChanged(int currentLives, int maxLives)
        {
            _gameDataService?.UpdateLives(currentLives);
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

        public void UnlockPowerUp(string powerUpName)
        {
            if (!_isInitialized || _gameDataService == null) return;
            
            _gameDataService.UpdatePowerUp(powerUpName, true);
            _autoSaveService?.RequestSave();
        }

        public bool HasPowerUp(string powerUpName)
        {
            if (!_isInitialized || _gameDataService == null) return false;
            
            return _gameDataService.HasPowerUp(powerUpName);
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
            _livesService?.ResetLives();
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
