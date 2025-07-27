using Health.Interfaces;
using UnityEngine;
using System.Linq;

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
            if (_damageable is null) return;
            if (((1 << source.layer) & sourceLayers) == 0) return;
            if (_damageConditions && !_damageConditions.CanBeDamagedBy(source)) return;

            var dealers = source.GetComponents<IDamageDealer>();
            if (dealers is {Length: 0}) return;

            IDamageDealer chosenDealer = dealers.OrderByDescending(d => d.GetDamageAmount()).FirstOrDefault();
            int maxAmount = chosenDealer?.GetDamageAmount() ?? 0;
            if (chosenDealer == null || maxAmount <= 0) return;
            _damageable.Damage(maxAmount, source);
        }
    }
}
