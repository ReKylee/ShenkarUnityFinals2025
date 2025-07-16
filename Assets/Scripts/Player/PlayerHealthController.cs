using System;
using GameEvents;
using GameEvents.Interfaces;
using Health;
using Health.Interfaces;
using Health.Views;
using Interfaces.Resettable;
using Managers.Interfaces;
using Player.Interfaces;
using UnityEngine;
using VContainer;

namespace Player
{
    public class PlayerHealthController : MonoBehaviour, IFullHealthSystem, ILivesSystem, IResettable
    {
        #region Fields
        [SerializeField] private int maxHp = 16;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private BarsHealthView healthView;
        [SerializeField] private TextHealthView livesView;

        // C# events for interface compliance
        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnLivesChanged;
        public event Action OnLivesEmpty;

        private PlayerLivesModel _model;
        private IEventBus _eventBus;
        private IResetManager _resetManager;
        #endregion

        #region VContainer Injection
        [Inject]
        public void Construct(IEventBus eventBus, IResetManager resetManager)
        {
            _eventBus = eventBus;
            _resetManager = resetManager;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeDefaultModel();
            // Subscribe model events to UnityEvents and C# events
            _model.OnHealthChanged += HandleHealthChanged;
            _model.OnLivesChanged += HandleLivesChanged;
            _model.OnLivesEmpty += HandleLivesEmpty;
        }

        private void Start()
        {
            _resetManager?.Register(this);
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnHealthChanged -= HandleHealthChanged;
                _model.OnLivesChanged -= HandleLivesChanged;
                _model.OnLivesEmpty -= HandleLivesEmpty;
                _model.Dispose();
            }
            _resetManager?.Unregister(this);
        }
        #endregion

        #region Event Handlers
        private void HandleHealthChanged(int hp, int maxHp)
        {
            OnHealthChanged?.Invoke(hp, maxHp);
            
            // Publish event through event bus
            _eventBus?.Publish(new PlayerHealthChangedEvent
            {
                CurrentHp = hp,
                MaxHp = maxHp,
                Damage = maxHp - hp,
                Timestamp = Time.time
            });
        }

        private void HandleLivesChanged(int lives, int maxLives)
        {
            OnLivesChanged?.Invoke(lives, maxLives);
            
            // Publish event through event bus
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = lives,
                MaxLives = maxLives,
                Timestamp = Time.time
            });
        }

        private void HandleLivesEmpty()
        {
            OnLivesEmpty?.Invoke();
            
            // Publish player death event
            _eventBus?.Publish(new PlayerDeathEvent
            {
                Timestamp = Time.time,
                DeathPosition = transform.position
            });
        }
        #endregion

        #region IHealthSystem Implementation
        public int MaxHp => _model?.MaxHp ?? maxHp;
        public int CurrentHp => _model?.CurrentHp ?? 0;

        public void Damage(int amount) => _model?.Damage(amount);
        public void Heal(int amount) => _model?.Heal(amount);
        public void SetHp(int hp) => _model?.SetHp(hp);
        #endregion

        #region ILivesSystem Implementation
        public int CurrentLives => _model?.CurrentLives ?? 0;
        public int MaxLives => _model?.MaxLives ?? maxLives;

        // ILivesSystem implementation
        public void Reset() => _model?.Reset();
        #endregion

        #region IResettable Implementation
        public void ResetState() => _model?.Reset();
        #endregion

        #region Initialization
        /// <summary>
        ///     Initialize with a default PlayerLivesModel implementation
        /// </summary>
        private void InitializeDefaultModel()
        {
            _model = new PlayerLivesModel(maxLives, maxHp);
            ConnectViewsToModel();
        }
        

        private void ConnectViewsToModel()
        {
            if (healthView) _model.OnHealthChanged += healthView.UpdateDisplay;
            if (livesView) _model.OnLivesChanged += livesView.UpdateDisplay;

            if (healthView) healthView.UpdateDisplay(_model.CurrentHp, _model.MaxHp);
            if (livesView) livesView.UpdateDisplay(_model.CurrentLives, _model.MaxLives);
        }
        #endregion
    }
}
