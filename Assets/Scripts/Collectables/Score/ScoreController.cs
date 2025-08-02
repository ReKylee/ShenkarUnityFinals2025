using System;
using Collectables.Counter;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private PaddedTextView scoreTextView;
        [SerializeField] private PaddedTextView fruitCountHealthView;
        private ICounterView _scoreTextView;
        private ICounterView _fruitCountHealthView;
        
        private IScoreService _scoreService;
        private void Awake()
        {
            _scoreTextView = scoreTextView;
            _fruitCountHealthView = fruitCountHealthView;
        }
        private void Start()
        {
            if (scoreTextView)
            {
                _scoreTextView.UpdateCountDisplay(_scoreService.CurrentScore);
            }

            if (fruitCountHealthView)
            {
                _fruitCountHealthView.UpdateCountDisplay(_scoreService.FruitCollectedCount);
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
                _scoreTextView.UpdateCountDisplay(_scoreService.CurrentScore);
            }

            if (fruitCountHealthView)
            {
                _fruitCountHealthView.UpdateCountDisplay(_scoreService.FruitCollectedCount);
            }
        }
    }
}
