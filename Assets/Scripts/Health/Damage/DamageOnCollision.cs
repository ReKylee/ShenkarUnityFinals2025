using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{
    [DisallowMultipleComponent]
    public class DamageOnCollision : MonoBehaviour
    {
        private IDamageDealer _dealer;
        private DamageConditionsComponent _damageConditions;

        [SerializeField] private LayerMask targetLayers = ~0; // All layers by default

        private void Awake()
        {
            _dealer = GetComponent<IDamageDealer>();
            _damageConditions = GetComponent<DamageConditionsComponent>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject target = collision.gameObject;
            if (((1 << target.layer) & targetLayers) == 0)
                return;
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null) return;

            if (_damageConditions && !_damageConditions.CanBeDamagedBy(target))
                return;

            int amount = _dealer?.GetDamageAmount() ?? 1;
            damageable.Damage(amount, gameObject);
        }
    }
}
