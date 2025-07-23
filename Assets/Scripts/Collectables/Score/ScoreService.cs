using System;
using Core.Data;
using VContainer;

namespace Collectables.Score
{
    public class ScoreService : IScoreService
    {
        private IGameDataService _gameDataService;
        public int CurrentScore => _gameDataService?.CurrentData?.score ?? 0;
        [Inject]
        public void Construct(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }
        public void AddScore(int amount)
        {
            _gameDataService?.UpdateScore(CurrentScore + amount);
        }
        public void ResetScore()
        {
            _gameDataService?.UpdateScore(0);
        }
    }
}
