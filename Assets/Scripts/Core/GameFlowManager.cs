using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Events;
using LevelSelection;
using LevelSelection.Services;
using Player.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Game Settings")] [SerializeField]
        private float restartDelay = 2f;

        [Header("Victory Settings")] [SerializeField]
        private string victorySceneName = "YouWonScene";

        [SerializeField] private float victoryTransitionDelay = 3f;

        private string _currentLevelName = "Unknown";
        private IEventBus _eventBus;
        private GameDataCoordinator _gameDataCoordinator;
        private float _levelStartTime;
        private ISceneLoadService _sceneLoadService;

        private GameState CurrentState { get; set; } = GameState.MainMenu;

        private void Start()
        {
            _currentLevelName = GetCurrentLevelName();

            // Only auto-start gameplay in actual level scenes
            // Other scenes (Level Select, Start Menu, etc.) will manage their own states
            if (ShouldAutoStartGameplay())
            {
                StartGameplay();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        [Inject]
        public void Construct(IEventBus eventBus, GameDataCoordinator gameDataCoordinator,
            ISceneLoadService sceneLoadService)
        {
            _eventBus = eventBus;
            _gameDataCoordinator = gameDataCoordinator;
            _sceneLoadService = sceneLoadService;
            SubscribeToEvents();
        }

        private static bool ShouldAutoStartGameplay()
        {
            // Only auto-start gameplay in actual level scenes
            string sceneName = SceneManager.GetActiveScene().name;
            return !sceneName.Equals("Level Select", StringComparison.OrdinalIgnoreCase) &&
                   !sceneName.Contains("Start") &&
                   !sceneName.Equals("YouWonScene", StringComparison.OrdinalIgnoreCase);
        }

        private void StartGameplay()
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

        public void StartLevel(string levelName)
        {
            _currentLevelName = levelName;
            Time.timeScale = 1;
            ChangeState(GameState.Playing);
            _levelStartTime = Time.time;

            _eventBus?.Publish(new LevelStartedEvent
            {
                LevelName = levelName,
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
            string currentSceneName = SceneManager.GetActiveScene().name;
            _sceneLoadService?.LoadLevel(currentSceneName);
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

        public void NavigateToLevelSelection()
        {
            try
            {
                ChangeState(GameState.LevelSelection);
                _sceneLoadService?.LoadLevel("Level Select");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameFlowManager] Failed to navigate to level selection: {e}");
            }
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

            // First, check if the level was already completed
            bool wasAlreadyCompleted = _gameDataCoordinator.IsLevelCompleted(levelEvent.LevelName);

            // Now, update the progress (marks as complete, updates best time, etc.)
            _gameDataCoordinator.UpdateLevelProgress(levelEvent.LevelName, true, levelEvent.CompletionTime);

            // If it's the first time completing this level, unlock the next one
            if (!wasAlreadyCompleted)
            {
                await UnlockNextLevelByIndex(levelEvent.LevelName);
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

        private async Task UnlockNextLevelByIndex(string completedLevelName)
        {
            Debug.Log($"[GameFlowManager] UnlockNextLevelByIndex called for: {completedLevelName}");

            var allLevels = await _gameDataCoordinator.DiscoverLevelsAsync();
            if (allLevels == null || allLevels.Count == 0)
            {
                Debug.LogWarning("[GameFlowManager] No levels discovered for unlocking next level");
                return;
            }

            Debug.Log($"[GameFlowManager] Found {allLevels.Count} levels total");

            LevelData completedLevel = allLevels.FirstOrDefault(l => l.levelName == completedLevelName);
            if (completedLevel == null)
            {
                Debug.LogWarning($"[GameFlowManager] Could not find completed level: {completedLevelName}");
                return;
            }

            Debug.Log($"[GameFlowManager] Completed level index: {completedLevel.levelIndex}");
            int nextLevelIndex = completedLevel.levelIndex + 1;

            if (nextLevelIndex < allLevels.Count)
            {
                LevelData nextLevel = allLevels.FirstOrDefault(l => l.levelIndex == nextLevelIndex);
                if (nextLevel != null)
                {
                    bool isAlreadyUnlocked = _gameDataCoordinator.IsLevelUnlocked(nextLevel.levelName);
                    Debug.Log(
                        $"[GameFlowManager] Next level '{nextLevel.levelName}' (index {nextLevelIndex}) - Already unlocked: {isAlreadyUnlocked}");

                    if (!isAlreadyUnlocked)
                    {
                        _gameDataCoordinator.UnlockLevel(nextLevel.levelName);
                        Debug.Log($"[GameFlowManager] Unlocked next level: {nextLevel.levelName}");
                    }
                    else
                    {
                        Debug.Log($"[GameFlowManager] Next level {nextLevel.levelName} was already unlocked");
                    }
                }
                else
                {
                    Debug.LogWarning($"[GameFlowManager] Could not find level with index {nextLevelIndex}");
                }
            }
            else
            {
                Debug.Log("[GameFlowManager] No more levels to unlock (completed level was the last one)");
            }
        }

        private async Task<bool> IsGameCompletedAsync()
        {
            try
            {
                // Get all available levels through GameDataCoordinator
                var allLevels = await _gameDataCoordinator.DiscoverLevelsAsync();

                if (allLevels == null || allLevels.Count == 0)
                {
                    Debug.LogWarning("[GameFlowManager] No levels found in discovery service");
                    return false;
                }

                // Get completed levels using wrapper method
                var completedLevels = _gameDataCoordinator?.GetCompletedLevels();
                if (completedLevels == null) return false;

                // Check if all levels are in the completed list
                int completedCount = 0;
                foreach (LevelData level in allLevels)
                {
                    if (completedLevels.Contains(level.levelName))
                    {
                        completedCount++;
                    }
                }

                Debug.Log(
                    $"[GameFlowManager] Game completion check: {completedCount}/{allLevels.Count} levels completed");

                if (completedCount >= allLevels.Count)
                {
                    Debug.Log("[GameFlowManager] All levels completed!");
                    return true;
                }

                var remainingLevels = allLevels.Where(l => !completedLevels.Contains(l.levelName))
                    .Select(l => l.levelName);

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

                _sceneLoadService?.LoadLevel(victorySceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameFlowManager] Failed to transition to victory scene: {e}");
                _sceneLoadService?.LoadLevel("Level Select");
            }
        }

        private async void ReturnToLevelSelectionAsync()
        {
            try
            {
                await Task.Delay((int)(2f * 1000));
                ChangeState(GameState.LevelSelection);
                _sceneLoadService?.LoadLevel("Level Select");
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
                // Use selective reset to preserve score and level progress
                _gameDataCoordinator?.ResetProgressData();
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

        private async void RestartLevelAfterDelayAsync(float delay)
        {
            try
            {
                await Task.Delay((int)(delay * 1000));
                string currentSceneName = SceneManager.GetActiveScene().name;
                _sceneLoadService?.LoadLevel(currentSceneName);
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
