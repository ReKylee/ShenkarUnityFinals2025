using Core.Events;
using TMPro;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class PopupTextService : MonoBehaviour, IPopupTextService
    {
        [SerializeField] private ScoreTextPool scoreTextPool;

        private IEventBus _eventBus;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnScoreChanged(ScoreChangedEvent scoreEvent)
        {
            // Display the added score amount as floating text
            ShowFloatingText(scoreEvent.Position, $"{scoreEvent.ScoreAmount}");
        }

        public void ShowFloatingText(Vector3 position, string text)
        {
            // Ensure this method is only called via ScoreChangedEvent
            TextMeshPro floatingText = scoreTextPool?.Get(text);
            if (floatingText)
            {
                floatingText.transform.position = position;
            }
        }
    }
}
