using Health.Interfaces;
using UnityEngine;

namespace Health.Damage.Conditions
{
    [DisallowMultipleComponent]
    public class OnlyProjectileCanDamage : MonoBehaviour, IDamageCondition
    {
        [SerializeField] private string projectileTag = "Projectiles";
        public bool CanBeDamagedBy(GameObject damager) => damager.CompareTag(projectileTag);
    }
}

