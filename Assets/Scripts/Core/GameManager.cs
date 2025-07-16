using System;
using GameEvents;
using GameEvents.Interfaces;
using UnityEngine;
using VContainer;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        #region Fields
        [Header("Game Settings")]
        [SerializeField] private string currentLevelName = "Level_01";
        [SerializeField] private bool autoStartGame = true;
        
        private GameState _currentState = GameState.MainMenu;
        private float _levelStartTime;
        private IEventBus _eventBus;
        #endregion

        #region Properties
        public GameState CurrentState => _currentState;
        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsPaused => _currentState == GameState.Paused;
        public bool HasEnded => _currentState == GameState.GameOver || _currentState == GameState.Victory;
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
            if (autoStartGame)
            {
                StartGame();
            }
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
        #endregion
    }
}
