using Core.Data;
using Player.Interfaces;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreService : IScoreService
    {
        private IGameDataService _gameDataService;
        private IPlayerLivesService _livesService;
        private const int OneUpThreshold = 30;
        public int CurrentScore => _gameDataService?.CurrentData?.score ?? 0;
        public void AddScore(int amount)
        {
            _gameDataService?.UpdateScore(CurrentScore + amount);
        }
        public void ResetScore()
        {
            _gameDataService?.UpdateScore(0);
        }
        public void AddFruitCollected(Vector3 collectPosition)
        {
            _gameDataService?.AddFruitCollected();
            int fruitCount = FruitCollectedCount;
            if (fruitCount > 0 && fruitCount % OneUpThreshold == 0)
            {
                _livesService?.AddLife(collectPosition);
                Debug.Log("One-up awarded! Player gained an extra life.");
            }
        }
        public int FruitCollectedCount => _gameDataService?.CurrentData?.fruitCollected ?? 0;
        [Inject]
        public void Construct(IGameDataService gameDataService, IPlayerLivesService livesService)
        {
            _gameDataService = gameDataService;
            _livesService = livesService;
        }
    }
}
