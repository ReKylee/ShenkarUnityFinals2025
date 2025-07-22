using Collectables._Base;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreCollectable : CollectibleBase
    {
        [SerializeField] private int scoreAmount = 1;
        private IScoreService _scoreService;
        private IPopupTextService _popupTextService;
        [Inject]
        public void Construct(IScoreService scoreService, IPopupTextService popupTextService)
        {
            _scoreService = scoreService;
            _popupTextService = popupTextService;
        }

        public override void OnCollect(GameObject collector)
        {
            Debug.Log("Score collected: " + gameObject.name);
            _popupTextService?.ShowFloatingText(transform.position, scoreAmount.ToString());
            _scoreService?.AddScore(scoreAmount);
        }
    }
}
