using Core;
using Core.Events;
using Player.Interfaces;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreService : IScoreService
    {
        private const int OneUpThreshold = 30;
        private IEventBus _eventBus;
        private GameDataCoordinator _gameDataCoordinator;
        private IPlayerLivesService _livesService;

        public int CurrentScore => _gameDataCoordinator?.GetCurrentScore() ?? 0;

        public void AddScore(int amount)
        {
            int previousScore = CurrentScore;
            int newScore = previousScore + amount;
            _gameDataCoordinator?.UpdateScore(newScore);

            // Publish score changed event for UI updates
            _eventBus?.Publish(new ScoreChangedEvent
            {
                Timestamp = Time.time,
                NewScore = newScore
            });
        }

        public void ResetScore()
        {
            _gameDataCoordinator?.UpdateScore(0);
        }

        public void AddFruitCollected(Vector3 collectPosition)
        {
            _gameDataCoordinator?.AddFruitCollected();
            int fruitCount = FruitCollectedCount;
            if (fruitCount > 0 && fruitCount % OneUpThreshold == 0)
            {
                _livesService?.AddLife(collectPosition);
                Debug.Log("One-up awarded! Player gained an extra life.");
            }

            // Publish event to update UI
            _eventBus?.Publish(new ScoreChangedEvent
            {
                Timestamp = Time.time,
                NewScore = CurrentScore
            });
        }

        public int FruitCollectedCount => _gameDataCoordinator?.GetFruitCollectedCount() ?? 0;

        [Inject]
        public void Construct(GameDataCoordinator gameDataCoordinator, IPlayerLivesService livesService,
            IEventBus eventBus)
        {
            _gameDataCoordinator = gameDataCoordinator;
            _livesService = livesService;
            _eventBus = eventBus;
        }
    }
}
