using System;
using GameEvents;
using GameEvents.Interfaces;
using Health;
using Health.Interfaces;
using Health.Views;
using UnityEngine;
using VContainer;

namespace Player
{
    public class PlayerHealthController : SimpleHealthController
    {
        private IEventBus _eventBus;

        #region VContainer Injection

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        #endregion

        #region Unity Lifecycle

        protected new void Start()
        {
            base.Start(); // Initialize the base SimpleHealthController

            // Subscribe to events after base initialization
            OnHealthChanged += HandleHealthChanged;
            OnLivesEmpty += HandleHealthEmpty;
        }

        protected new void OnDestroy()
        {
            OnHealthChanged -= HandleHealthChanged;
            OnLivesEmpty -= HandleHealthEmpty;
            base.OnDestroy();
        }

        #endregion

        #region Event Handlers

        private void HandleHealthChanged(int hp, int maxHp)
        {
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
