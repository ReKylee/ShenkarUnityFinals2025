using Health.Interfaces;
using UnityEngine;

namespace Health.Damage.Conditions
{
    [DisallowMultipleComponent]
    public class OnlyInvincibleCanDamage : MonoBehaviour, IDamageCondition
    {
        public bool CanBeDamagedBy(GameObject damager)
        {
            IInvincibility inv = damager.GetComponent<IInvincibility>();
            return inv is { IsInvincible: true };
        }
    }
}
