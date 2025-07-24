namespace Collectables.Score
{
    public interface IScoreService
    {
        public int CurrentScore { get; }
        public void AddScore(int amount);
        public void ResetScore();
    }

}
