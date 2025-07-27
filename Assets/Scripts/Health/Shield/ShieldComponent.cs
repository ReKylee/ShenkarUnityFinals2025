using System;
using Health.Interfaces;
using UnityEngine;

namespace Health.Shield
{
    [DisallowMultipleComponent]
    public class ShieldComponent : MonoBehaviour, IShield, IDamageDealer
    {
        public bool IsActive { get; private set; }
        public event Action<int> OnShieldBroken;

        public void ActivateShield()
        {
            IsActive = true;
        }
        public void BreakShield(int damageAmount)
        {
            if (!IsActive) return;
            IsActive = false;
            OnShieldBroken?.Invoke(damageAmount);
        }
        public int GetDamageAmount() => IsActive ? 1 : 0;
    }
}
