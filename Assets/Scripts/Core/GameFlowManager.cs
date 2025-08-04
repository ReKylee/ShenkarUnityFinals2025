using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Events;
using Player.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float restartDelay = 2f;
        
        [Header("Victory Settings")]
        [SerializeField] private string victorySceneName = "YouWonScene";
        [SerializeField] private float victoryTransitionDelay = 3f;

        private string _currentLevelName = "Unknown";
        private float _levelStartTime;
        private IEventBus _eventBus;
        private GameDataCoordinator _gameDataCoordinator;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public bool IsPlaying => CurrentState == GameState.Playing;

        [Inject]
        public void Construct(IEventBus eventBus, GameDataCoordinator gameDataCoordinator)
        {
            _eventBus = eventBus;
            _gameDataCoordinator = gameDataCoordinator;
            SubscribeToEvents();
        }

        private void Start()
        {
            _currentLevelName = GetCurrentLevelName();

            if (autoStartGame)
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void StartGame()
        {
            Time.timeScale = 1;
            ChangeState(GameState.Playing);
            _levelStartTime = Time.time;

            _eventBus?.Publish(new LevelStartedEvent
            {
                LevelName = _currentLevelName,
                Timestamp = Time.time
            });
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
                ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
                ChangeState(GameState.Playing);
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void HandlePlayerDeath(Vector3 deathPosition)
        {
            _eventBus?.Publish(new PlayerDeathEvent
            {
                DeathPosition = deathPosition,
                Timestamp = Time.time
            });
        }

        public void CompleteLevel(string currentLevelName)
        {
            float completionTime = Time.time - _levelStartTime;
            
            _eventBus?.Publish(new LevelCompletedEvent
            {
                LevelName = currentLevelName,
                CompletionTime = completionTime,
                Timestamp = Time.time
            });
        }

        public void RequestLevelLoad(string levelName, string sceneName)
        {
            _eventBus?.Publish(new LevelLoadRequestedEvent
            {
                Timestamp = Time.time,
                LevelName = levelName,
                SceneName = sceneName
            });
        }

        public void SelectLevel(string levelName, int levelIndex)
        {
            _eventBus?.Publish(new LevelSelectedEvent
            {
                Timestamp = Time.time,
                LevelName = levelName,
                LevelIndex = levelIndex
            });
        }

        public void NavigateLevel(int previousIndex, int newIndex, Vector2 direction)
        {
            _eventBus?.Publish(new LevelNavigationEvent
            {
                Timestamp = Time.time,
                PreviousIndex = previousIndex,
                NewIndex = newIndex,
                Direction = direction
            });
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<GameOverEvent>(OnGameOver);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus?.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<GameOverEvent>(OnGameOver);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus?.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        }

        private void OnGameOver(GameOverEvent gameOverEvent)
        {
            ChangeState(GameState.GameOver);
            RestartLevelAfterDelayAsync(restartDelay);
        }

        private async void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            ChangeState(GameState.Victory);
            
            // Update level progress through GameDataCoordinator
            _gameDataCoordinator?.UpdateLevelProgress(levelEvent.LevelName, true, levelEvent.CompletionTime);
            
            // Also update it in GameData for immediate checking
            var gameData = _gameDataCoordinator?.GetCurrentData();
            if (gameData != null && !gameData.completedLevels.Contains(levelEvent.LevelName))
            {
                gameData.completedLevels.Add(levelEvent.LevelName);
                
                // Unlock next level if it exists
                UnlockNextLevel(levelEvent.LevelName, gameData);
            }
            
            if (await IsGameCompletedAsync())
            {
                Debug.Log("[GameFlowManager] All levels completed! Transitioning to YouWonScene...");
                TransitionToVictorySceneAsync();
            }
            else
            {
                Debug.Log($"[GameFlowManager] Level {levelEvent.LevelName} completed. Returning to level selection...");
                ReturnToLevelSelectionAsync();
            }
        }

        private void UnlockNextLevel(string completedLevelName, GameData gameData)
        {
            // Simple next level unlocking logic - you can make this more sophisticated
            if (completedLevelName.Contains("Level_"))
            {
                var levelNumber = ExtractLevelNumber(completedLevelName);
                var nextLevelName = $"Level_{levelNumber + 1:D2}";
                
                if (!gameData.unlockedLevels.Contains(nextLevelName))
                {
                    gameData.unlockedLevels.Add(nextLevelName);
                    Debug.Log($"[GameFlowManager] Unlocked next level: {nextLevelName}");
                }
            }
        }

        private int ExtractLevelNumber(string levelName)
        {
            // Extract number from level name like "Level_01" -> 1
            var parts = levelName.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int levelNum))
            {
                return levelNum;
            }
            return 0;
        }

        private async Task<bool> IsGameCompletedAsync()
        {
            try
            {
                // Check using GameData completed levels through GameDataCoordinator
                var gameData = _gameDataCoordinator?.GetCurrentData();
                if (gameData == null) return false;

                // Get all available levels through GameDataCoordinator
                var allLevels = await _gameDataCoordinator.DiscoverLevelsAsync();
                
                if (allLevels == null || allLevels.Count == 0)
                {
                    Debug.LogWarning("[GameFlowManager] No levels found in discovery service");
                    return false;
                }

                // Check if all levels are in the completed list
                var completedCount = 0;
                foreach (var level in allLevels)
                {
                    if (gameData.completedLevels.Contains(level.levelName))
                    {
                        completedCount++;
                    }
                }
                
                Debug.Log($"[GameFlowManager] Game completion check: {completedCount}/{allLevels.Count} levels completed");
                
                if (completedCount >= allLevels.Count)
                {
                    Debug.Log("[GameFlowManager] All levels completed!");
                    return true;
                }

                var remainingLevels = allLevels.Where(l => !gameData.completedLevels.Contains(l.levelName)).Select(l => l.levelName);
                Debug.Log($"[GameFlowManager] Remaining levels: {string.Join(", ", remainingLevels)}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameFlowManager] Error checking game completion: {e}");
                return false;
            }
        }

        private async void TransitionToVictorySceneAsync()
        {
            try
            {
                await Task.Delay((int)(victoryTransitionDelay * 1000));
                
                _eventBus?.Publish(new GameCompletedEvent
                {
                    Timestamp = Time.time,
                    FinalLevelName = _currentLevelName
                });
                
                SceneManager.LoadScene(victorySceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameFlowManager] Failed to transition to victory scene: {e}");
                SceneManager.LoadScene("Level Select");
            }
        }

        private async void ReturnToLevelSelectionAsync()
        {
            try
            {
                await Task.Delay((int)(2f * 1000));
                SceneManager.LoadScene("Level Select");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameFlowManager] Failed to return to level selection: {e}");
            }
        }

        private void OnPlayerLivesChanged(PlayerLivesChangedEvent livesEvent)
        {
            bool lostLife = livesEvent.PreviousLives > livesEvent.CurrentLives;
            bool isGameOver = livesEvent.CurrentLives == 0;

            if (isGameOver)
            {
                ChangeState(GameState.GameOver);
                _gameDataCoordinator?.ResetAllData();
                _eventBus?.Publish(new GameOverEvent { Timestamp = Time.time });
            }

            if (lostLife)
            {
                _eventBus?.Publish(new PlayerDeathEvent
                {
                    DeathPosition = PlayerLocator.PlayerTransform.position,
                    Timestamp = Time.time
                });
            }

            if (isGameOver || lostLife)
            {
                Time.timeScale = 0.01f;
                RestartLevelAfterDelayAsync(restartDelay);
            }
        }

        private static async void RestartLevelAfterDelayAsync(float delay)
        {
            try
            {
                await Task.Delay((int)(delay * 1000));
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameFlowManager] Failed to restart level after delay: {e}");
            }
        }

        private void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState oldState = CurrentState;
            CurrentState = newState;

            _eventBus?.Publish(new GameStateChangedEvent
            {
                PreviousState = oldState,
                NewState = newState,
                Timestamp = Time.time
            });
        }

        private string GetCurrentLevelName() => SceneManager.GetActiveScene().name;
    }
}
