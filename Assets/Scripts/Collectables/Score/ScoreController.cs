using UnityEngine;
using VContainer;
using Core.Events;

namespace Collectables.Score
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private ScoreView view;

        private IEventBus _eventBus;

        #region VContainer Injection

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        #endregion


        private void OnEnable()
        {
            _eventBus?.Subscribe<ScoreChangedEvent>(HandleScoreChangedEvent);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<ScoreChangedEvent>(HandleScoreChangedEvent);
        }

        private void HandleScoreChangedEvent(ScoreChangedEvent scoreEvent)
        {
            if (view)
            {
                view.UpdateCountDisplay(scoreEvent.TotalScore);
            }
        }
    }
}
