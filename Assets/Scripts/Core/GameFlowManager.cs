using Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core
{
    /// <summary>
    /// All aspects of game flow including state transitions, level management, and game progression.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        #region Fields
        [Header("Game Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float restartDelay = 2f;

        private GameState _currentState = GameState.MainMenu;
        private string _currentLevelName = "Unknown";
        private float _levelStartTime;
        private IEventBus _eventBus;
        #endregion

        #region Properties
        public GameState CurrentState => _currentState;
        public bool IsPlaying => _currentState == GameState.Playing;
        #endregion

        #region VContainer Injection
        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
        }
        #endregion

        #region Unity Lifecycle
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
        #endregion

        #region Public API - Game State Control
        public void StartGame()
        {
            ChangeState(GameState.Playing);
            _levelStartTime = Time.time;

            PublishLevelStartedEvent();
        }

        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
                ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
                ChangeState(GameState.Playing);
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        #endregion

        #region Public API - Game Flow Control
        public void HandlePlayerDeath(Vector3 deathPosition)
        {
            // Publish player death event
            _eventBus?.Publish(new PlayerDeathEvent
            {
                DeathPosition = deathPosition,
                Timestamp = Time.time
            });

            HandleLevelFailureInternal("Player died");
        }


        // Internal method to handle level failure without relying on events
        private void HandleLevelFailureInternal(string reason)
        {
            // Change state if we're not already in GameOver
            if (_currentState != GameState.GameOver)
            {
                ChangeState(GameState.GameOver);
            }

            // Schedule the restart
            Invoke(nameof(RestartLevel), restartDelay);
        }

        public void CompleteLevel(float completionTime)
        {
            _eventBus?.Publish(new LevelCompletedEvent
            {
                LevelName = _currentLevelName,
                CompletionTime = completionTime,
                Timestamp = Time.time
            });
        }

        // This method is now redundant because GameOverEvent is triggered by ChangeState(GameState.GameOver)
        // Keeping for backward compatibility
        public void HandleGameOver()
        {
            ChangeState(GameState.GameOver);
        }
        #endregion

        #region Event Handlers
        private void SubscribeToEvents()
        {
            // GameOverEvent is now a redundant event - it's automatically published when state changes to GameOver
            // Keeping the subscription for backward compatibility or external sources of GameOverEvent
            _eventBus?.Subscribe<GameOverEvent>(OnGameOver);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<GameOverEvent>(OnGameOver);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void OnGameOver(GameOverEvent gameOverEvent)
        {
            ChangeState(GameState.GameOver);
            Invoke(nameof(RestartLevel), restartDelay * 2f);
        }


        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            ChangeState(GameState.Victory);
            // Load next level or show victory screen logic would go here
        }
        #endregion

        #region Private Methods
        private void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;

            GameState oldState = _currentState;
            _currentState = newState;

            // Publish state change event
            _eventBus?.Publish(new GameStateChangedEvent
            {
                PreviousState = oldState,
                NewState = newState,
                Timestamp = Time.time
            });

            // If we're changing to GameOver state, also publish a GameOverEvent for backward compatibility
            if (newState == GameState.GameOver)
            {
                _eventBus?.Publish(new GameOverEvent { Timestamp = Time.time });
            }
        }

        private string GetCurrentLevelName()
        {
            return SceneManager.GetActiveScene().name;
        }

        private void PublishLevelStartedEvent()
        {
            _eventBus?.Publish(new LevelStartedEvent
            {
                LevelName = _currentLevelName,
                Timestamp = Time.time
            });
        }
        #endregion
    }
}
