using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data;
using Core.Events;
using Core.Services;
using LevelSelection;
using LevelSelection.Services;
using UnityEngine;
using VContainer;

namespace Core
{
    public class GameDataCoordinator : MonoBehaviour
    {

#if UNITY_EDITOR
        [Header("Debug")] [SerializeField] private bool resetGameData;
#endif

        private IAutoSaveService _autoSaveService;
        private IEventBus _eventBus;
        private IGameDataService _gameDataService;
        private bool _isInitialized;
        private ILevelDiscoveryService _levelDiscoveryService;


        private void Update()
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.Update();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus?.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            _eventBus?.Unsubscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
            _eventBus?.Unsubscribe<LevelNavigationEvent>(OnLevelNavigation);

            _autoSaveService?.ForceSave();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.OnApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!_isInitialized || _autoSaveService == null) return;

            _autoSaveService.OnApplicationPause(pauseStatus);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (resetGameData)
            {
                if (_gameDataService != null)
                {
                    _gameDataService.ResetAllData();
                    Debug.Log("Game data has been reset.");
                }
                else
                {
                    Debug.LogWarning("GameDataService not available. Cannot reset game data.");
                }

                resetGameData = false;
            }
        }
#endif

        [Inject]
        public void Construct(
            IGameDataService gameDataService,
            IEventBus eventBus,
            IAutoSaveService autoSaveService,
            ILevelDiscoveryService levelDiscoveryService)
        {
            _gameDataService = gameDataService;
            _eventBus = eventBus;
            _autoSaveService = autoSaveService;
            _levelDiscoveryService = levelDiscoveryService;
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
        }

        private void SaveData()
        {
            _gameDataService?.SaveData();
        }

        private void OnPlayerDied(PlayerDeathEvent deathEvent)
        {
            _autoSaveService?.RequestSave();
        }

        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            _gameDataService?.UpdateBestTime(levelEvent.LevelName, levelEvent.CompletionTime);
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

      

        private void OnLevelSelected(LevelSelectedEvent levelEvent)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.selectedLevelIndex = levelEvent.LevelIndex;
                gameData.currentLevel = levelEvent.LevelName;
            }
        }

        private void OnLevelNavigation(LevelNavigationEvent navigationEvent)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.selectedLevelIndex = navigationEvent.NewIndex;
            }
        }

        // Public API for other systems to request data operations
        public void UpdateLives(int lives)
        {
            if (!_isInitialized) return;
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                PreviousLives = GetCurrentLives(),
                CurrentLives = lives,
                Timestamp = Time.time
            });
            _gameDataService?.UpdateLives(lives);
        }

        public void UpdateCurrentLevel(string levelName)
        {
            if (!_isInitialized) return;
            _gameDataService?.UpdateCurrentLevel(levelName);
        }

        public void UpdateLevelProgress(string levelName, bool isCompleted, float completionTime)
        {
            if (!_isInitialized) return;
            _gameDataService?.UpdateLevelProgress(levelName, isCompleted, completionTime);
        }

        public void UpdateScore(int score)
        {
            if (!_isInitialized) return;
            _eventBus?.Publish(new ScoreChangedEvent
            {
                NewScore = score,
                Timestamp = Time.time
            });
            _gameDataService?.UpdateScore(score);
        }

        public void AddFruitCollected()
        {
            if (!_isInitialized) return;
            _gameDataService?.AddFruitCollected();
        }

        public async Task<List<LevelData>> DiscoverLevelsAsync()
        {
            if (!_isInitialized || _gameDataService == null || _levelDiscoveryService == null)
                return new List<LevelData>();

            return await _gameDataService.GetLevelDataAsync(_levelDiscoveryService);
        }

        public GameData GetCurrentData() => !_isInitialized ? null : _gameDataService?.CurrentData;

        public void ResetAllData()
        {
            if (!_isInitialized) return;
            _gameDataService?.ResetAllData();
        }

        public void ResetProgressData()
        {
            if (!_isInitialized) return;
            _gameDataService?.ResetProgressData();
        }

        // Wrapper methods for GameData operations 
        public bool IsLevelUnlocked(string levelName) => GetCurrentData()?.IsLevelUnlocked(levelName) ?? false;

        public bool IsLevelCompleted(string levelName) => GetCurrentData()?.IsLevelCompleted(levelName) ?? false;

        public float GetLevelBestTime(string levelName) =>
            GetCurrentData()?.GetLevelBestTime(levelName) ?? float.MaxValue;

        public int GetLevelBestScore(string levelName) => GetCurrentData()?.GetLevelBestScore(levelName) ?? 0;

        // Additional wrapper methods to avoid GetCurrentData() calls
        public int GetCurrentLives() => GetCurrentData()?.lives ?? GameData.MaxLives;

        public List<string> GetUnlockedLevels() => GetCurrentData()?.unlockedLevels ?? new List<string> { "Level_01" };

        public List<string> GetCompletedLevels() => GetCurrentData()?.completedLevels ?? new List<string>();

        public int GetSelectedLevelIndex() => GetCurrentData()?.selectedLevelIndex ?? 0;

        public void UnlockLevel(string levelName)
        {
            if (!_isInitialized) return;
            _gameDataService?.UnlockLevel(levelName);
        }

        public int GetCurrentScore() => GetCurrentData()?.score ?? 0;

        public int GetFruitCollectedCount() => GetCurrentData()?.fruitCollected ?? 0;
    }
}
