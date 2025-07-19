using System;
using Health.Interfaces;
using Health.Models;
using Health.Views;
using UnityEngine;
using VContainer;

namespace Health
{
    public class SimpleHealthController : MonoBehaviour, IFullHealthSystem
    {
        [SerializeField] private int maxHp = 3;

        private IFullHealthSystem _model;

        private void Awake()
        {
                InitializeDefaultModel();
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

       
    }
}
