using System;
using Health.Interfaces;

namespace Health.Models
{
    public class HealthModel : IFullHealthSystem
    {
        public HealthModel(int currentHp, int maxHp)
        {
            CurrentHp = currentHp;
            MaxHp = maxHp;
        }

        public int CurrentHp { get; private set; }
        public int MaxHp { get; }
        public event Action<int, int> OnHealthChanged;
        public event Action OnLivesEmpty;

        public void SetHp(int hp)
        {
            int oldHp = CurrentHp;
            CurrentHp = Math.Max(0, hp);

            if (oldHp != CurrentHp)
            {
                NotifyHealthChanged();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            int oldHp = CurrentHp;
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);

            if (oldHp != CurrentHp)
            {
                NotifyHealthChanged();
            }
        }

        public void Damage(int amount)
        {
            if (amount <= 0) return;

            int oldHp = CurrentHp;
            CurrentHp = Math.Max(CurrentHp - amount, 0);

            if (oldHp != CurrentHp)
            {
                NotifyHealthChanged();

                if (CurrentHp <= 0)
                {
                    OnLivesEmpty?.Invoke();
                }
            }
        }

        private void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
