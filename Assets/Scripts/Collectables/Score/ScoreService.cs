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
            if (_gameDataService == null) return;

            _gameDataService.UpdateScore(CurrentScore + amount);
            ScoreChanged?.Invoke(CurrentScore);
        }
        public void ResetScore()
        {
            if (_gameDataService == null) return;

            _gameDataService.UpdateScore(0);
            ScoreChanged?.Invoke(CurrentScore);
        }
        public event Action<int> ScoreChanged;
    }
}
