using Collectables._Base;
using UnityEngine;
using VContainer;
using Core.Events;

namespace Collectables.Score
{
    public class ScoreCollectable : CollectibleBase
    {
        [SerializeField] private int scoreAmount = 1;
        private IScoreService _scoreService;
        private IEventBus _eventBus;

        [Inject]
        public void Construct(IScoreService scoreService, IEventBus eventBus)
        {
            _scoreService = scoreService;
            _eventBus = eventBus;
            Debug.Log("ScoreCollectable constructed with service: " + _scoreService?.GetType().Name);
        }

        public override void OnCollect(GameObject collector)
        {
            Debug.Log("Score collected: " + gameObject.name);
            _scoreService.AddScore(scoreAmount);

            _eventBus?.Publish(new ScoreChangedEvent
            {
                ScoreAmount = scoreAmount,
                TotalScore = _scoreService.CurrentScore + scoreAmount,
                Position = transform.position
            });
        }
    }
}
