using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{
    // Example: Attach this to hazards or damaging objects
    [DisallowMultipleComponent]
    public class DamageOnCollision : MonoBehaviour
    {
        private IDamageDealer _dealer;
        private DamageConditionsComponent _damageConditions;

        private void Awake()
        {
            _dealer = GetComponent<IDamageDealer>();
            _damageConditions = GetComponent<DamageConditionsComponent>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject target = collision.gameObject;
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null) return;

            // If hazard has conditions, check them
            if (_damageConditions && !_damageConditions.CanBeDamagedBy(target))
                return;

            int amount = _dealer?.GetDamageAmount() ?? 1;
            damageable.Damage(amount, gameObject);
        }
    }
}
