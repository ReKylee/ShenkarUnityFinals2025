using Core.Events;
using UnityEngine;

namespace Core.Flow
{
    public class GameFlowController : IGameFlowController
    {
        private readonly IEventBus _eventBus;
        private string _currentLevelName = "Unknown"; 

        public GameFlowController(IEventBus eventBus)
        {
            _eventBus = eventBus;
            
            // Subscribe to level started events to track current level
            _eventBus?.Subscribe<LevelStartedEvent>(OnLevelStarted);
        }

        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            _currentLevelName = levelEvent.LevelName;
            Debug.Log($"[GameFlowController] Level started: {_currentLevelName}");
        }

        public void HandlePlayerDeath()
        {
            _eventBus?.Publish(new LevelFailedEvent
            {
                LevelName = _currentLevelName,
                FailureReason = "Player died",
                Timestamp = Time.time
            });
        }

        public void HandleLevelCompletion(float completionTime)
        {
            _eventBus?.Publish(new LevelCompletedEvent
            {
                LevelName = _currentLevelName,
                CompletionTime = completionTime,
                Timestamp = Time.time
            });
        }

        public void HandleGameOver()
        {
            _eventBus?.Publish(new GameOverEvent
            {
                Timestamp = Time.time
            });
        }

        private void HandleAllLivesLost()
        {
            HandleGameOver();
        }
    }
}
