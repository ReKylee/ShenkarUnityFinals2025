using Core.Events;
using Health;
using Health.Views;
using UnityEngine;
using VContainer;
using Core.Data;

namespace Player
{
    public class PlayerLivesController : SimpleHealthController
    {
        [SerializeField] private TextHealthView healthView;
        private IEventBus _eventBus;
        private IGameDataService _gameDataService;

        #region VContainer Injection

        [Inject]
        public void Construct(IEventBus eventBus, IGameDataService gameDataService)
        {
            _eventBus = eventBus;
            _gameDataService = gameDataService;
        }

        #endregion

        #region Unity Lifecycle

        protected void Start()
        {
            SetHp(_gameDataService?.CurrentData.lives ?? MaxHp);
            healthView.UpdateDisplay(CurrentHp, MaxHp);
            // Subscribe to events after base initialization
            OnHealthChanged += HandleHealthChanged;
            OnLivesEmpty += HandleHealthEmpty;
            // Publish initial lives to GameData on start
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = CurrentHp,
                MaxLives = MaxHp,
                Timestamp = Time.time
            });
        }

        protected void OnDestroy()
        {
            OnHealthChanged -= HandleHealthChanged;
            OnLivesEmpty -= HandleHealthEmpty;
        }

        #endregion

        #region Event Handlers

        private void HandleHealthChanged(int hp, int maxHp)
        {
            healthView.UpdateDisplay(hp, maxHp);
            _eventBus?.Publish(new PlayerLivesChangedEvent()
            {
                CurrentLives = hp,
                MaxLives = maxHp,
                Timestamp = Time.time
            });
        }

        private void HandleHealthEmpty()
        {
            _eventBus?.Publish(new PlayerDeathEvent
            {
                Timestamp = Time.time,
                DeathPosition = transform.position
            });
        }

        #endregion

    }
}
