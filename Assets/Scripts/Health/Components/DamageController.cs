using Health.Interfaces;
using UnityEngine;

namespace Health.Components
{
    public class DamageController : MonoBehaviour
    {
        private IDamageable _damageable;

        public bool IsInvulnerable { get; set; }

        private void Awake()
        {
            _damageable = GetComponent<IDamageable>();
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            TryDamageDealer(other.gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamageDealer(other.gameObject);
        }
        private void TryDamageDealer(GameObject other)
        {
            if (_damageable == null) return;
            if (IsInvulnerable) return;

            if (other.TryGetComponent(out IDamageDealer dealer))
            {
                Debug.Log("Damage dealer " + other.name);
                _damageable.Damage(dealer.GetDamageAmount());
            }
        }
    }
}
