using Core;
using Core.Events;
using Health;
using Health.Core;
using Health.Interfaces;
using Player.Interfaces;
using UnityEngine;
using VContainer;

namespace Player.Components
{
    public class PlayerHealthController : HealthComponent, IBypassableDamageable
    {
        [SerializeField] private BarsHealthView healthView;
        private IEventBus _eventBus;
        private GameFlowManager _gameFlowManager;
        private IPlayerLivesService _livesService;
        private IShield _shield;
        private IInvincibility _invincibility;

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

        private new void Awake()
        {
            base.Awake();
            _shield = GetComponent<IShield>();
            _invincibility = GetComponent<IInvincibility>();
        }

        protected void Start()
        {
            healthView.UpdateDisplay(CurrentHp, MaxHp);
            OnHealthChanged += HandleHealthChanged;
            OnDeath += HandleHealthEmpty;
        }

        protected void OnDestroy()
        {
            OnHealthChanged -= HandleHealthChanged;
            OnDeath -= HandleHealthEmpty;
        }

        #endregion

        #region Event Handlers

        private void HandleHealthChanged(int hp, int maxHp)
        {
            healthView.UpdateDisplay(hp, maxHp);
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

            if (_livesService.TryUseLife())
            {
                Debug.Log(
                    $"[PlayerHealthController] Used a life, waiting for scene reload. Lives remaining: {_livesService.CurrentLives}");
                return;
            }

            Debug.Log("[PlayerHealthController] No lives left, calling HandlePlayerDeath.");
            _gameFlowManager.HandlePlayerDeath(transform.position);
        }

        #endregion


        #region Damage Handling

        public override void Damage(int amount, GameObject source = null)
        {
            if (_invincibility is { IsInvincible: true })
                return;
            if (_shield is { IsActive: true })
            {
                Debug.Log("[PlayerHealthController] Shield active, breaking shield.");
                _shield.BreakShield(amount);
                return;
            }

            base.Damage(amount, source);
        }

        /// <summary>
        ///     Damage that bypasses transformation shield (used by continuous damage, etc.)
        /// </summary>
        public void DamageBypass(int amount)
        {
            base.Damage(amount, gameObject);
        }

        #endregion

    }
}
