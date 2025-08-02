using Collectables.Counter;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private PaddedTextView scoreTextView;
        [SerializeField] private PaddedTextView fruitCountHealthView;

        private IScoreService _scoreService;

        private void Start()
        {
            if (scoreTextView)
            {
                scoreTextView.UpdateCountDisplay(_scoreService.CurrentScore);
            }
            if (fruitCountHealthView)
            {
                fruitCountHealthView.UpdateCountDisplay(_scoreService.FruitCollectedCount);
            }
        }

        private void OnEnable()
        {
            ScoreCollectable.OnScoreCollected += HandleScoreCollected;
        }

        private void OnDisable()
        {
            ScoreCollectable.OnScoreCollected -= HandleScoreCollected;
        }

        #region VContainer Injection

        [Inject]
        public void Construct(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        #endregion

        private void HandleScoreCollected(int scoreAmount, Vector3 position)
        {
            _scoreService.AddScore(scoreAmount);
            _scoreService.AddFruitCollected(position);
            if (scoreTextView)
            {
                scoreTextView.UpdateCountDisplay(_scoreService.CurrentScore);
            }
            if (fruitCountHealthView)
            {
                fruitCountHealthView.UpdateCountDisplay(_scoreService.FruitCollectedCount);
            }
        }
    }
}
