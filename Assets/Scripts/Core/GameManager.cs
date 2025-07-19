using Core.Events;
using UnityEngine;
using VContainer;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        #region Fields
        [Header("Game Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float restartDelay = 2f;
        
        private GameState _currentState = GameState.MainMenu;
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

        #region Public API
        public void StartGame()
        {
            ChangeState(GameState.Playing);
            _levelStartTime = Time.time;
            
            _eventBus?.Publish(new LevelStartedEvent
            {
                LevelName = GetCurrentLevelName(),
                Timestamp = Time.time
            });
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
            Debug.Log("GameManager: RestartLevel called - Reloading scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        #endregion

        #region Event Handlers
        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<GameOverEvent>(OnGameOver);
            _eventBus?.Subscribe<LevelFailedEvent>(OnLevelFailed);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<GameOverEvent>(OnGameOver);
            _eventBus?.Unsubscribe<LevelFailedEvent>(OnLevelFailed);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void OnGameOver(GameOverEvent gameOverEvent)
        {
            Debug.Log("GameManager: Game Over - All lives lost");
            ChangeState(GameState.GameOver);
            Invoke(nameof(RestartLevel), restartDelay * 2f);
        }

        private void OnLevelFailed(LevelFailedEvent levelEvent)
        {
            Debug.Log("GameManager: Level failed - Player died");
            ChangeState(GameState.GameOver);
            
            Debug.Log($"GameManager: Scheduling level restart in {restartDelay} seconds");
            Invoke(nameof(RestartLevel), restartDelay);
        }

        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            Debug.Log($"GameManager: Level completed in {levelEvent.CompletionTime:F2} seconds");
            ChangeState(GameState.Victory);
            // Handle level completion (next level, victory screen, etc.)
        }
        #endregion

        #region Private Methods
        private void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;
            
            Debug.Log($"GameManager: State changed from {_currentState} to {newState}");
            _currentState = newState;
        }

        private string GetCurrentLevelName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        #endregion
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }
}
