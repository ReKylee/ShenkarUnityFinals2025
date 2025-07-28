using Health.Interfaces;
using UnityEngine;

namespace Health.Invincibility
{
    [DisallowMultipleComponent]
    public class InvincibilityComponent : MonoBehaviour, IInvincibility, IDamageDealer
    {

        public int GetDamageAmount()
        {
            Debug.Log("[InvincibilityComponent] GetDamageAmount called. IsInvincible: " + IsInvincible, gameObject);
            return IsInvincible ? 9999 : 0;
        }
        public bool IsInvincible { get; private set; }
        public void SetInvincible(bool value)
        {
            IsInvincible = value;
        }
    }
}
