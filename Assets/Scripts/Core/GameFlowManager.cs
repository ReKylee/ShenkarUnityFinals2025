using System;
using System.Threading.Tasks;
using Core.Events;
using Player.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core
{
    /// <summary>
    ///     All aspects of game flow including state transitions, level management, and game progression.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {

        #region VContainer Injection

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
        }

        #endregion

        #region Fields

        [Header("Game Settings")] [SerializeField]
        private bool autoStartGame = true;

        [SerializeField] private float restartDelay = 2f;

        private string _currentLevelName = "Unknown";
        private float _levelStartTime;
        private IEventBus _eventBus;

        #endregion

        #region Properties

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        public bool IsPlaying => CurrentState == GameState.Playing;

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

        #endregion

        #region Public API - Game Flow Control

        public void HandlePlayerDeath(Vector3 deathPosition)
        {
            _eventBus?.Publish(new PlayerDeathEvent
            {
                DeathPosition = deathPosition,
                Timestamp = Time.time
            });
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

        #endregion

        #region Event Handlers

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


        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            ChangeState(GameState.Victory);
        }

        private void OnPlayerLivesChanged(PlayerLivesChangedEvent livesEvent)
        {
            bool lostLife = livesEvent.PreviousLives > livesEvent.CurrentLives;
            bool isGameOver = livesEvent.CurrentLives == 0;

            if (isGameOver)
            {
                ChangeState(GameState.GameOver);
                Debug.Log($"[GameFlowManager] Game Over: Player is out of lives");
                _eventBus?.Publish(new GameOverEvent { Timestamp = Time.time });
            }
            if (lostLife)
            {
                Debug.Log($"[GameFlowManager] Player lost a life. Remaining lives: {livesEvent.CurrentLives}");
                _eventBus?.Publish(new PlayerDeathEvent
                {
                    DeathPosition = PlayerLocator.PlayerTransform.position,
                    Timestamp = Time.time
                });
            }
            if (isGameOver || lostLife)
            {
                Time.timeScale = 0;
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
        #endregion

        #region Private Methods

        private void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState oldState = CurrentState;
            CurrentState = newState;

            // Publish state change event
            _eventBus?.Publish(new GameStateChangedEvent
            {
                PreviousState = oldState,
                NewState = newState,
                Timestamp = Time.time
            });
        }

        private string GetCurrentLevelName() => SceneManager.GetActiveScene().name;

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
