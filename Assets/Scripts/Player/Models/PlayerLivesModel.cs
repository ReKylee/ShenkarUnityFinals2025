using System;
using Health.Interfaces;
using Health.Models;

namespace Player
{
    public class PlayerLivesModel : IDamageable
    {

        private readonly HealthModel _healthModel;

        public PlayerLivesModel(int maxLives, int hpPerLife)
        {
            MaxLives = maxLives;
            CurrentLives = maxLives;
            _healthModel = new HealthModel(hpPerLife, hpPerLife);
            _healthModel.OnHealthChanged += HealthChanged;
            _healthModel.OnLivesEmpty += LoseLife;

        }
        public int CurrentLives { get; private set; }
        public int MaxLives { get; }
        public int CurrentHp => _healthModel.CurrentHp;
        public int MaxHp => _healthModel.MaxHp;

        public void Damage(int amount) => _healthModel.Damage(amount);

        public event Action<int, int> OnHealthChanged;
        public event Action OnLivesEmpty;

        public void Heal(int amount) => _healthModel.Heal(amount);

        public void SetHp(int hp) => _healthModel.SetHp(hp);
        public void Dispose()
        {
            _healthModel.OnHealthChanged -= HealthChanged;
            _healthModel.OnLivesEmpty -= LoseLife;
        }
        private void HealthChanged(int hp, int maxHp) => OnHealthChanged?.Invoke(hp, maxHp);
        public event Action<int, int> OnLivesChanged;

        private void LoseLife()
        {
            CurrentLives = Math.Max(0, CurrentLives - 1);
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
            if (CurrentLives > 0)
            {
                _healthModel.SetHp(MaxHp);
                OnHealthChanged?.Invoke(CurrentHp, MaxHp);
                return;
            }
            OnLivesEmpty?.Invoke();
        }

        public void Reset()
        {
            CurrentLives = MaxLives;
            _healthModel.SetHp(MaxHp);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
        }
    }
}
