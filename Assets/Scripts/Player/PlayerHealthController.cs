using System;
using Health;
using Health.Interfaces;
using Health.Views;
using Interfaces.Resettable;
using Managers;
using Player.Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerHealthController : MonoBehaviour, IFullHealthSystem, ILivesSystem, IResettable
    {
        [SerializeField] private int maxHp = 3;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private SliderHealthView healthView;
        [SerializeField] private TextHealthView livesView;

        [SerializeField] private bool createModelOnAwake = true;
        private PlayerLivesModel _model;

        private void Awake()
        {
            if (createModelOnAwake)
            {
                InitializeDefaultModel();
            }
        }

        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            _model?.Dispose();

            ResetManager.Instance?.Unregister(this);
        }

        // IHealthSystem implementation
        public int MaxHp => _model?.MaxHp ?? maxHp;
        public int CurrentHp => _model?.CurrentHp ?? 0;

        public event Action OnEmpty
        {
            add
            {
                if (_model != null) _model.OnEmpty += value;
            }
            remove
            {
                if (_model != null) _model.OnEmpty -= value;
            }
        }

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

        public void Damage(int amount) => _model?.Damage(amount);

        public void Heal(int amount) => _model?.Heal(amount);

        public void SetHp(int hp) => _model?.SetHp(hp);

        public int CurrentLives => _model?.CurrentLives ?? 0;
        public int MaxLives => _model?.MaxLives ?? maxLives;

        public event Action<int, int> OnLivesChanged
        {
            add
            {
                if (_model != null) _model.OnLivesChanged += value;
            }
            remove
            {
                if (_model != null) _model.OnLivesChanged -= value;
            }
        }

        // ILivesSystem implementation
        public void Reset() => _model?.Reset();

        // IResettable implementation
        public void ResetState() => _model?.Reset();

        /// <summary>
        ///     Initialize with a default PlayerLivesModel implementation
        /// </summary>
        private void InitializeDefaultModel()
        {
            _model = new PlayerLivesModel(maxLives, maxHp);
            ConnectViewsToModel();
        }

        /// <summary>
        ///     Set a custom lives model implementation (for dependency injection)
        /// </summary>
        public void SetLivesModel(PlayerLivesModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            ConnectViewsToModel();
        }

        private void ConnectViewsToModel()
        {
            if (healthView) _model.OnHealthChanged += healthView.UpdateDisplay;
            if (livesView) _model.OnLivesChanged += livesView.UpdateDisplay;

            if (healthView) healthView.UpdateDisplay(_model.CurrentHp, _model.MaxHp);
            if (livesView) livesView.UpdateDisplay(_model.CurrentLives, _model.MaxLives);
        }
    }
}
