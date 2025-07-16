using System;
using Health.Interfaces;
using Health.Models;
using Health.Views;
using Interfaces.Resettable;
using Managers.Interfaces;
using UnityEngine;
using VContainer;

namespace Health
{
    public class SimpleHealthController : MonoBehaviour, IFullHealthSystem, IResettable
    {
        [SerializeField] private int maxHp = 3;
        [SerializeField] private TextHealthView healthView;
        [SerializeField] private bool createModelOnAwake = true;

        private IFullHealthSystem _model;
        private IResetManager _resetManager;

        #region VContainer Injection
        [Inject]
        public void Construct(IResetManager resetManager)
        {
            _resetManager = resetManager;
        }
        #endregion

        private void Awake()
        {
            if (createModelOnAwake)
            {
                InitializeDefaultModel();
            }
        }

        private void Start()
        {
            _resetManager?.Register(this);
        }

        private void OnDestroy()
        {
            _resetManager?.Unregister(this);
        }

        public int MaxHp => _model?.MaxHp ?? maxHp;
        public int CurrentHp => _model?.CurrentHp ?? 0;

        public event Action<int, int> OnHealthChanged
        {
            add
            {
                if (_model != null) _model.OnHealthChanged += value;
            }
            remove
            {
                if (_model != null) _model.OnHealthChanged -= value;
            }
        }

        public event Action OnLivesEmpty
        {
            add
            {
                if (_model != null) _model.OnLivesEmpty += value;
            }
            remove
            {
                if (_model != null) _model.OnLivesEmpty -= value;
            }
        }

        public void Damage(int amount) => _model?.Damage(amount);

        public void Heal(int amount) => _model?.Heal(amount);

        public void SetHp(int hp) => _model?.SetHp(hp);

        public void ResetState() => _model?.SetHp(_model.MaxHp);

        /// <summary>
        ///     Initialize with a default HealthModel implementation
        /// </summary>
        private void InitializeDefaultModel() => _model = new HealthModel(maxHp, maxHp);

        /// <summary>
        ///     Set a custom health model implementation (for dependency injection)
        /// </summary>
        public void SetHealthModel(IFullHealthSystem model) =>
            _model = model ?? throw new ArgumentNullException(nameof(model));
    }
}
