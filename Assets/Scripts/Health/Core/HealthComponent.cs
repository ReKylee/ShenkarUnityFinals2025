using System;
using Health.Interfaces;
using UnityEngine;

namespace Health.Core
{
    [DisallowMultipleComponent]
    public class HealthComponent : MonoBehaviour, IHealth
    {
        [SerializeField] private int maxHp = 3;

        protected void Awake()
        {
            CurrentHp = maxHp;
        }
        public int MaxHp => maxHp;
        public int CurrentHp { get; private set; }
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public virtual void Damage(int amount, GameObject source = null)
        {
            if (CurrentHp <= 0) return;
            int prevHp = CurrentHp;
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
            if (CurrentHp == 0 && prevHp > 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
