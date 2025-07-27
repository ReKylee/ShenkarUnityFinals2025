using Health.Interfaces;
using UnityEngine;

namespace Health.Damage.Conditions
{
    [DisallowMultipleComponent]
    public class OnlyShieldedCanDamage : MonoBehaviour, IDamageCondition
    {
        public bool CanBeDamagedBy(GameObject damager)
        {
            IShield shield = damager.GetComponent<IShield>();
            return shield is { IsActive: true };
        }
    }
}
