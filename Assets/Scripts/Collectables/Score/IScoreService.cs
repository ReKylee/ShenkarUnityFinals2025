using UnityEngine;

namespace Collectables.Score
{
    public interface IScoreService
    {
        public int CurrentScore { get; }
        public int FruitCollectedCount { get; }
        public void AddScore(int amount);
        public void ResetScore();
        void AddFruitCollected(Vector3 collectPosition);
    }

}
