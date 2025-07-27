using Health.Interfaces;
using UnityEngine;

namespace Health.Invincibility
{
    [DisallowMultipleComponent]
    public class InvincibilityComponent : MonoBehaviour, IInvincibility, IDamageDealer
    {
        public bool IsInvincible { get; private set; }
        public void SetInvincible(bool value)
        {
            IsInvincible = value;
        }

        public int GetDamageAmount() => IsInvincible ? 9999 : 0;
    }
}
