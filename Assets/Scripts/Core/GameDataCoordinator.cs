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
        [Header("Settings")]
        [SerializeField] private int defaultLives = 3;

        private IGameDataService _gameDataService;
        private ILivesService _livesService;
        private IEventBus _eventBus;
        private IAutoSaveService _autoSaveService;
        private IGameFlowController _gameFlowController;

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
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Subscribe to events
            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            
            // Connect services
            _livesService.OnLivesChanged += OnLivesChanged;
            _gameDataService.OnDataChanged += OnGameDataChanged;
            _autoSaveService.OnSaveRequested += SaveData;
            
            // Initialize lives from saved data
            _livesService.SetLives(_gameDataService.CurrentData.lives);
        }

        private void Update()
        {
            _autoSaveService.Update();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            
            _livesService.OnLivesChanged -= OnLivesChanged;
            _gameDataService.OnDataChanged -= OnGameDataChanged;
            _autoSaveService.OnSaveRequested -= SaveData;
            
            _autoSaveService.ForceSave();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _autoSaveService.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _autoSaveService.OnApplicationFocus(hasFocus);
        }
        #endregion

        #region Event Handlers
        private void OnPlayerDied(PlayerDeathEvent deathEvent)
        {
            Debug.Log("GameDataCoordinator: Player died, delegating to GameFlowController");
            _gameFlowController.HandlePlayerDeath();
            _autoSaveService.RequestSave();
        }

        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            _gameDataService.UpdateBestTime(levelEvent.CompletionTime);
            _autoSaveService.RequestSave();
        }

        private void OnLivesChanged(int currentLives, int maxLives)
        {
            _gameDataService.UpdateLives(currentLives);
        }

        private void OnGameDataChanged(Data.GameData gameData)
        {
            _autoSaveService.RequestSave();
        }
        #endregion

        #region Public API
        public void AddScore(int points)
        {
            int newScore = _gameDataService.CurrentData.score + points;
            _gameDataService.UpdateScore(newScore);
        }

        public void AddCoins(int coinCount)
        {
            int newCoins = _gameDataService.CurrentData.coins + coinCount;
            _gameDataService.UpdateCoins(newCoins);
        }

        public void UnlockPowerUp(string powerUpName)
        {
            _gameDataService.UpdatePowerUp(powerUpName, true);
            _autoSaveService.RequestSave();
        }

        public bool HasPowerUp(string powerUpName)
        {
            return _gameDataService.HasPowerUp(powerUpName);
        }

        public GameData GetCurrentData()
        {
            return _gameDataService.CurrentData;
        }

        public void ResetAllData()
        {
            _gameDataService.ResetAllData();
            _livesService.ResetLives();
        }
        #endregion

        #region Private Methods
        private void SaveData()
        {
            _gameDataService.SaveData();
        }
        #endregion
    }
}
