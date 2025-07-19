using Core.Events;
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
        private readonly IGameEventPublisher _gameEventPublisher;
        private readonly string _currentLevelName;

        public GameFlowController(
            IGameEventPublisher gameEventPublisher,
            string currentLevelName = "Level_01")
        {
            _gameEventPublisher = gameEventPublisher;
            _currentLevelName = currentLevelName;
            
            // Subscribe to lives events
        }

        public void HandlePlayerDeath()
        {
            
            // Always publish level failed when player dies (regardless of lives remaining)
            // GameManager will handle the restart, and lives system will handle game over separately
            _gameEventPublisher.PublishLevelFailed(_currentLevelName, "Player died");
            
            // Note: If this was the last life, LivesService will trigger OnAllLivesLost event
            // which will call HandleAllLivesLost() -> HandleGameOver() -> PublishGameOver()
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

       
    }
}
