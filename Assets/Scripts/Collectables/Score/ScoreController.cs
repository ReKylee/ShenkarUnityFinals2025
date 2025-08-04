using Collectables.Counter;
using Core.Events;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private PaddedTextView scoreTextView;
        [SerializeField] private PaddedTextView fruitCountHealthView;
        private IEventBus _eventBus;
        private ICounterView _fruitCountHealthView;

        private IScoreService _scoreService;
        private ICounterView _scoreTextView;

        private void Awake()
        {
            _scoreTextView = scoreTextView;
            _fruitCountHealthView = fruitCountHealthView;
        }

        private void Start()
        {
            _scoreTextView?.UpdateCountDisplay(_scoreService.CurrentScore);
            _fruitCountHealthView?.UpdateCountDisplay(_scoreService.FruitCollectedCount);
        }

        private void OnEnable()
        {
            ScoreCollectable.OnScoreCollected += HandleScoreCollected;
            _eventBus?.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnDisable()
        {
            ScoreCollectable.OnScoreCollected -= HandleScoreCollected;
            _eventBus?.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnScoreChanged(ScoreChangedEvent scoreEvent)
        {
            _scoreTextView.UpdateCountDisplay(scoreEvent.NewScore);
            _fruitCountHealthView.UpdateCountDisplay(_scoreService.FruitCollectedCount);
        }

        #region VContainer Injection

        [Inject]
        public void Construct(IScoreService scoreService, IEventBus eventBus)
        {
            _scoreService = scoreService;
            _eventBus = eventBus;
        }

        #endregion

        private void HandleScoreCollected(int scoreAmount, Vector3 collectedPosition)
        {
            _scoreService.AddScore(scoreAmount);
            _scoreService.AddFruitCollected(collectedPosition);
        }
    }
}
