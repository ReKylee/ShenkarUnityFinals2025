using Health.Interfaces;
using UnityEngine;

namespace Health.Damage.Conditions
{
    [DisallowMultipleComponent]
    public class OnlyProjectileCanDamage : MonoBehaviour, IDamageCondition
    {
        [SerializeField] private LayerMask projectileLayers = ~0;
        public bool CanBeDamagedBy(GameObject damager)
        {
            return ((1 << damager.layer) & projectileLayers) != 0;
        }
    }
}
