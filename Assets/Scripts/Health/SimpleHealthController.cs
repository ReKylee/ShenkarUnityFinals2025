﻿using System;
using Health.Interfaces;
using Health.Models;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Health
{
    public class SimpleHealthController : MonoBehaviour, IFullHealthSystem, IResettable
    {
        [SerializeField] private int maxHp = 3;

        [SerializeField] private bool createModelOnAwake = true;
        private IFullHealthSystem _model;

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
            ResetManager.Instance?.Unregister(this);
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
