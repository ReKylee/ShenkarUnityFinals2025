using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private ScoreView view;

        private IScoreService _scoreService;

        #region VContainer Injection

        [Inject]
        public void Construct(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        #endregion

        private void Start()
        {
            if (view && _scoreService is not null)
            {
                view.UpdateCountDisplay(_scoreService.CurrentScore);
            }
            
        }

        private void OnEnable()
        {
            if (_scoreService is not null)
                _scoreService.ScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            if (_scoreService is not null)
                _scoreService.ScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            view?.UpdateCountDisplay(newScore);
        }
    }
}
