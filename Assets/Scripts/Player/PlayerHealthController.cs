using Core.Events;
using Health;
using UnityEngine;
using VContainer;

namespace Player
{
    public class PlayerHealthController : SimpleHealthController
    {
        [SerializeField] private BarsHealthView healthView;
        private IEventBus _eventBus;
        
        #region VContainer Injection

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        #endregion

        #region Unity Lifecycle

        protected void Start()
        {
            healthView.UpdateDisplay(CurrentHp, MaxHp);
            // Subscribe to events after base initialization
            OnHealthChanged += HandleHealthChanged;
            OnLivesEmpty += HandleHealthEmpty;
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
            healthView.UpdateDisplay(CurrentHp, MaxHp);
            _eventBus?.Publish(new PlayerHealthChangedEvent
            {
                CurrentHp = hp,
                MaxHp = maxHp,
                Damage = maxHp - hp,
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
