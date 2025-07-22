using Core;
using Core.Events;
using Health;
using Health.Components;
using Health.Interfaces;
using Player.Services;
using UnityEngine;
using VContainer;

namespace Player.Components
{
    public class PlayerHealthController : SimpleHealthController, IBypassableDamageable
    {
        [SerializeField] private BarsHealthView healthView;

        private IEventBus _eventBus;
        private IPlayerLivesService _livesService;
        private IDamageShield _damageShield;
        private GameFlowManager _gameFlowManager;

        #region VContainer Injection

        [Inject]
        public void Construct(IEventBus eventBus, IPlayerLivesService livesService, GameFlowManager gameFlowManager)
        {
            _eventBus = eventBus;
            _livesService = livesService;
            _gameFlowManager = gameFlowManager;
        }

        #endregion

        #region Unity Lifecycle

        protected void Start()
        {
            _damageShield = GetComponent<IDamageShield>();
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
            // Check for null dependencies
            if (_livesService == null)
            {
                Debug.LogError("[PlayerHealthController] _livesService is null. Ensure it is properly injected.");
                return;
            }

            if (!_gameFlowManager)
            {
                Debug.LogError("[PlayerHealthController] _gameFlowManager is null. Ensure it is properly injected.");
                return;
            }

            // Try to use a life through the service
            if (_livesService.TryUseLife())
            {
                ResetState();
                Debug.Log(
                    $"[PlayerHealthController] Used a life, restored health. Lives remaining: {_livesService.CurrentLives}");

                return;
            }

            _gameFlowManager.HandlePlayerDeath(transform.position);
        }

        #endregion

        #region Transformation Handling

        public void ActivateShield() => _damageShield.Activate();
        public void DeactivateShield() => _damageShield.Deactivate();
        
        #endregion

        #region Damage Handling

        public new void Damage(int amount)
        {
            if (_damageShield.TryAbsorbDamage(amount))
            {
                Debug.Log("[PlayerHealthController] Transformation absorbed damage!");
                return;
            }

            base.Damage(amount);
        }

        /// <summary>
        /// Damage that bypasses transformation shield (used by continuous damage, etc.)
        /// </summary>
        public void DamageBypass(int amount)
        {
            base.Damage(amount);
        }

        #endregion

    }
}
