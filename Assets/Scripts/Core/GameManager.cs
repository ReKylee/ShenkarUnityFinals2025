using Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        #region Fields
        [Header("Game Settings")]
        [SerializeField] private string currentLevelName = "Level_01";
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float respawnDelay = 2f;
        
        private GameState _currentState = GameState.MainMenu;
        private float _levelStartTime;
        private IEventBus _eventBus;
        private bool _isInitialized = false;
        #endregion

        #region Properties
        public GameState CurrentState => _currentState;
        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsPaused => _currentState == GameState.Paused;
        public bool HasEnded => _currentState is GameState.GameOver or GameState.Victory;
        #endregion

        #region VContainer Injection
        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _isInitialized = true;
            
            // Initialize after dependency injection
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
            
            if (autoStartGame)
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            if (!_isInitialized || _eventBus == null) return;
            
            _eventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            _eventBus.Unsubscribe<LevelFailedEvent>(OnLevelFailed);
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            // Subscribe to game over events (when all lives are lost)
            _eventBus?.Subscribe<GameOverEvent>(OnGameOver);
            // Subscribe to level failed events (when player dies but has lives remaining)
            _eventBus?.Subscribe<LevelFailedEvent>(OnLevelFailed);
            // Subscribe to player death events to handle respawn/restart
            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
        }
        #endregion

        #region Public API
        public void StartGame()
        {
            if (!_isInitialized || _eventBus == null) return;
            
            if (_currentState != GameState.MainMenu && _currentState != GameState.Restarting)
                return;

            ChangeState(GameState.Playing);
            _levelStartTime = Time.time;
            
            _eventBus.Publish(new LevelStartedEvent
            {
                LevelName = currentLevelName,
                Timestamp = Time.time
            });
        }

        public void PauseGame()
        {
            if (!_isInitialized) return;
            
            if (_currentState != GameState.Playing)
                return;

            ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (!_isInitialized) return;
            
            if (_currentState != GameState.Paused)
                return;

            ChangeState(GameState.Playing);
        }

        public void CompleteLevel()
        {
            if (!_isInitialized || _eventBus == null) return;
            
            if (_currentState != GameState.Playing)
                return;

            ChangeState(GameState.Victory);
            
            _eventBus.Publish(new LevelCompletedEvent
            {
                LevelName = currentLevelName,
                CompletionTime = Time.time - _levelStartTime,
                Timestamp = Time.time
            });
        }

        public void FailLevel(string reason = "Player died")
        {
            if (!_isInitialized || _eventBus == null) return;
            
            if (_currentState != GameState.Playing)
                return;

            ChangeState(GameState.GameOver);
            
            _eventBus.Publish(new LevelFailedEvent
            {
                LevelName = currentLevelName,
                FailureReason = reason,
                Timestamp = Time.time
            });
        }

        public void RestartGame()
        {
            if (!_isInitialized) return;
            
            ChangeState(GameState.Restarting);
            Invoke(nameof(DelayedRestart), 1f);
        }

        public void HandlePlayerDeath()
        {
            if (!_isInitialized || _eventBus == null) return;
            
            if (_currentState != GameState.Playing)
                return;

            ChangeState(GameState.GameOver);
            
            _eventBus.Publish(new PlayerDeathEvent
            {
                Timestamp = Time.time,
                DeathPosition = Vector3.zero // Will be set by PlayerHealthController
            });

            // Restart the level after a delay (Adventure Island 3 style)
            Invoke(nameof(RestartLevel), respawnDelay);
        }
        #endregion

        #region Event Handlers
        private void OnGameOver(GameOverEvent gameOverEvent)
        {
            if (!_isInitialized || _eventBus == null) return;
            
            // All lives lost - true game over
            ChangeState(GameState.GameOver);
            
            _eventBus.Publish(new LevelFailedEvent
            {
                LevelName = currentLevelName,
                FailureReason = "All lives lost",
                Timestamp = Time.time
            });

            // Here you could show game over screen, return to main menu, etc.
            // For now, restart after longer delay
            Invoke(nameof(RestartToMainMenu), respawnDelay * 2f);
        }

        private void OnLevelFailed(LevelFailedEvent levelFailedEvent)
        {
            if (!_isInitialized) return;
            
            // Handle level failure (e.g., player death) but allow for restarts if lives remain
            // This could simply be a state change, or you could add more logic here
            if (_currentState == GameState.Playing)
            {
                // If we have a respawn system, we might want to respawn the player here
                // For now, let's just change the state to GameOver which will trigger a restart
                ChangeState(GameState.GameOver);
            }
        }

        private void OnPlayerDeath(PlayerDeathEvent playerDeathEvent)
        {
            if (!_isInitialized || _eventBus == null) return;
            
            // Handle player death - restart level after delay
            ChangeState(GameState.GameOver);
            
            // Restart the level after a delay
            Invoke(nameof(RestartLevel), respawnDelay);
        }
        #endregion

        #region Private Methods
        private void ChangeState(GameState newState)
        {
            if (!_isInitialized || _eventBus == null) return;
            
            if (_currentState == newState)
                return;

            var previousState = _currentState;
            _currentState = newState;
            
            _eventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState,
                Timestamp = Time.time
            });
        }

        private void DelayedRestart()
        {
            StartGame();
        }

        private void RestartLevel()
        {
            // Simple scene reload - everything resets automatically
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void RestartToMainMenu()
        {
            // Reset all persistent data for true restart
            // No need for static reset - we can reset the data directly
            Invoke(nameof(DelayedGameOverReset), 0.1f);
        }

        private void DelayedGameOverReset()
        {
            // Find the GameDataCoordinator and reset it
            var gameDataCoordinator = FindAnyObjectByType<GameDataCoordinator>();
            gameDataCoordinator?.ResetAllData();
            
            // Could load main menu scene here instead
            RestartLevel();
        }
        #endregion
    }
}
