using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{

    [DisallowMultipleComponent]
    public class TakeDamageOnCollision : MonoBehaviour
    {
        private IDamageable _damageable;
        private DamageConditionsComponent _damageConditions;
        [SerializeField] private LayerMask sourceLayers = ~0;

        private void Awake()
        {
            TryGetComponent(out _damageable);
            TryGetComponent(out _damageConditions);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject source = collision.gameObject;
            if (_damageable == null) return;
            if (((1 << source.layer) & sourceLayers) == 0) return;
            
            if (!source.TryGetComponent(out IDamageDealer dealer)) return;
            
            if (_damageConditions && !_damageConditions.CanBeDamagedBy(source)) return;
            
            int amount = dealer.GetDamageAmount();
            if (amount > 0)
                _damageable.Damage(amount, source);
        }
    }
}
