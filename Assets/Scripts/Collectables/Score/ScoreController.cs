using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private ScoreView view;

        private IScoreService _scoreService;

        private void Start()
        {
            if (view)
            {
                view.UpdateCountDisplay(_scoreService.CurrentScore);
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
            if (view)
            {
                view.UpdateCountDisplay(_scoreService.CurrentScore + scoreAmount);
            }

            _scoreService.AddScore(scoreAmount);
        }
    }
}
