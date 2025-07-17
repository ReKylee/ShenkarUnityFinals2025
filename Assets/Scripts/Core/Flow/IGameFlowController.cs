using Core.Events;
using Core.Lives;
using UnityEngine;

namespace Core.Flow
{
    public interface IGameFlowController
    {
        void HandlePlayerDeath();
        void HandleLevelCompletion(float completionTime);
        void HandleGameOver();
    }

    public class GameFlowController : IGameFlowController
    {
        private readonly ILivesService _livesService;
        private readonly IGameEventPublisher _gameEventPublisher;
        private readonly string _currentLevelName;

        public GameFlowController(
            ILivesService livesService, 
            IGameEventPublisher gameEventPublisher,
            string currentLevelName = "Level_01")
        {
            _livesService = livesService;
            _gameEventPublisher = gameEventPublisher;
            _currentLevelName = currentLevelName;
            
            // Subscribe to lives events
            _livesService.OnAllLivesLost += HandleAllLivesLost;
        }

        public void HandlePlayerDeath()
        {
            _livesService.LoseLife();
            
            // If player still has lives, trigger level restart
            if (_livesService.CurrentLives > 0)
            {
                _gameEventPublisher.PublishLevelFailed(_currentLevelName, "Player died");
            }
            // If no lives left, OnAllLivesLost event will handle game over
        }

        public void HandleLevelCompletion(float completionTime)
        {
            _gameEventPublisher.PublishLevelCompleted(_currentLevelName, completionTime);
        }

        public void HandleGameOver()
        {
            _gameEventPublisher.PublishGameOver();
        }

        private void HandleAllLivesLost()
        {
            HandleGameOver();
        }

        public void Dispose()
        {
            _livesService.OnAllLivesLost -= HandleAllLivesLost;
        }
    }
}
