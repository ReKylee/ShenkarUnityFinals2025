using Collectables._Base;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreCollectable : CollectibleBase
    {
        [SerializeField] private int scoreAmount = 1;
        private IScoreService _scoreService;
        [Inject]
        public void Construct(IScoreService scoreService)
        {
            _scoreService = scoreService;
            
           
        }

        public override void OnCollect(GameObject collector)
        {
            _scoreService?.AddScore(scoreAmount);
        }
    }
}
