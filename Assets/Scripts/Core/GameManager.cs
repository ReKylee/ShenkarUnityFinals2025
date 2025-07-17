using System;
using GameEvents;
using GameEvents.Interfaces;
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
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Subscribe to game over events (when all lives are lost)
            _eventBus?.Subscribe<GameOverEvent>(OnGameOver);
            
            if (autoStartGame)
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<GameOverEvent>(OnGameOver);
        }
        #endregion

        #region Public API
        public void StartGame()
        {
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
            if (_currentState != GameState.Playing)
                return;

            ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (_currentState != GameState.Paused)
                return;

            ChangeState(GameState.Playing);
        }

        public void CompleteLevel()
        {
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
            ChangeState(GameState.Restarting);
            Invoke(nameof(DelayedRestart), 1f);
        }

        public void HandlePlayerDeath()
        {
            if (_currentState != GameState.Playing)
                return;

            ChangeState(GameState.GameOver);
            
            _eventBus?.Publish(new PlayerDeathEvent
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
            // All lives lost - true game over
            ChangeState(GameState.GameOver);
            
            _eventBus?.Publish(new LevelFailedEvent
            {
                LevelName = currentLevelName,
                FailureReason = "All lives lost",
                Timestamp = Time.time
            });

            // Here you could show game over screen, return to main menu, etc.
            // For now, restart after longer delay
            Invoke(nameof(RestartToMainMenu), respawnDelay * 2f);
        }
        #endregion

        #region Private Methods
        private void ChangeState(GameState newState)
        {
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
            // Find the PersistentDataManager and reset it
            var persistentData = FindAnyObjectByType<PersistentDataManager>();
            persistentData?.ResetAllData();
            
            // Could load main menu scene here instead
            RestartLevel();
        }
        #endregion
    }
}
