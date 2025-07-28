using System;
using UnityEngine;

namespace Health.Interfaces
{
    public interface IHealable
    {
        void Heal(int amount);
    }

    public interface IDamageable
    {
        void Damage(int amount, GameObject source = null);
    }

    public interface IHealthEvents
    {
        event Action<int, int> OnHealthChanged;
        event Action OnDeath;
    }

    public interface IHealth : IHealable, IDamageable, IHealthEvents
    {
        int CurrentHp { get; }
        int MaxHp { get; }
    }

    public interface IDamageDealer
    {
        int GetDamageAmount();
    }

    public interface IShield
    {
        bool IsActive { get; }
        void ActivateShield();
        void BreakShield(int damageAmount);
        event Action<int> OnShieldBroken;
    }

    public interface IInvincibility
    {
        bool IsInvincible { get; }
        void SetInvincible(bool value);
    }

    public interface IDamageCondition
    {
        bool CanBeDamagedBy(GameObject damager);
    }

    public interface IBypassableDamageable
    {
        void DamageBypass(int amount);
    }
}
