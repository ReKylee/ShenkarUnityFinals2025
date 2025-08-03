using Core.Data;
using Core.Events;
using Core.Services;
using UnityEngine;
using VContainer;

namespace Core
{
    public class GameDataCoordinator : MonoBehaviour
    {
        [SerializeField] private float autoSaveInterval = 30;
        private IAutoSaveService _autoSaveService;
        private IEventBus _eventBus;
        private IGameDataService _gameDataService;
        private bool _isInitialized;

        // Track previous values to detect changes
        private int _previousLives = -1;

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

            Initialize();
        }

        private void Initialize()
        {
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus?.Subscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Subscribe<LevelSelectedEvent>(OnLevelSelected);
            _eventBus?.Subscribe<LevelNavigationEvent>(OnLevelNavigation);

            if (_gameDataService != null)
            {
                _gameDataService.OnDataChanged += OnGameDataChanged;
                _previousLives = _gameDataService.CurrentData.lives;
            }

            if (_autoSaveService != null)
            {
                _autoSaveService.OnSaveRequested += SaveData;
                _autoSaveService.SaveInterval = autoSaveInterval;
                _autoSaveService.IsEnabled = true;
            }
        }

        private void SaveData()
        {
            _gameDataService?.SaveData();
        }

        private void Update()
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.Update();
        }

        private void OnDestroy()
        {
            if (!_isInitialized) return;

            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<PlayerDeathEvent>(OnPlayerDied);
                _eventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
                _eventBus.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
                _eventBus.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
                _eventBus.Unsubscribe<LevelNavigationEvent>(OnLevelNavigation);
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
            if (stateEvent.NewState is GameState.Victory or GameState.GameOver)
            {
                _autoSaveService?.ForceSave();
            }
        }

        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            _gameDataService?.UpdateCurrentLevel(levelEvent.LevelName);
        }

        private void OnGameDataChanged(GameData newData)
        {
            // Check if lives changed and publish the appropriate event
            if (_previousLives != newData.lives)
            {
                _eventBus?.Publish(new PlayerLivesChangedEvent
                {
                    PreviousLives = _previousLives,
                    CurrentLives = newData.lives,
                    MaxLives = GameData.MaxLives,
                    Timestamp = Time.time
                });

                _previousLives = newData.lives;
            }

            _autoSaveService?.RequestSave();
        }

        private void OnLevelSelected(LevelSelectedEvent levelEvent)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.selectedLevelIndex = levelEvent.LevelIndex;
                gameData.currentLevel = levelEvent.LevelName;
            }
        }

        private void OnLivesChanged(PlayerLivesChangedEvent livesEvent)
        {
            _gameDataService?.UpdateLives(livesEvent.CurrentLives);
        }

        private void OnLevelNavigation(LevelNavigationEvent navigationEvent)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.selectedLevelIndex = navigationEvent.NewIndex;
            }
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
    }
}
