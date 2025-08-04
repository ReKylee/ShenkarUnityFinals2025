using Core;
using Health.Core;
using Health.Interfaces;
using Health.Views;
using Player.Interfaces;
using UnityEngine;
using VContainer;

namespace Player.Components
{
    public class PlayerHealthController : HealthComponent, IBypassableDamageable
    {
        [SerializeField] private BarsHealthView healthView;
        private GameFlowManager _gameFlowManager;
        private IInvincibility _invincibility;
        private IPlayerLivesService _livesService;
        private IShield _shield;
        public IHealthView HealthView { get; private set; }

        #region VContainer Injection

        [Inject]
        public void Construct(IPlayerLivesService livesService, GameFlowManager gameFlowManager)
        {
            _livesService = livesService;
            _gameFlowManager = gameFlowManager;
        }

        #endregion

        #region Unity Lifecycle

        private new void Awake()
        {
            base.Awake();
            HealthView = healthView;
            _shield = GetComponent<IShield>();
            _invincibility = GetComponent<IInvincibility>();
        }

        protected void Start()
        {
            HealthView.UpdateDisplay(CurrentHp, MaxHp);
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
            HealthView.UpdateDisplay(hp, maxHp);
        }

        private void HandleHealthEmpty()
        {

            if (_livesService.TryUseLife())
            {
                return;
            }
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
