using Health.Interfaces;
using UnityEngine;

namespace Health.Components
{
    /// <summary>
    /// Abstract base implementing the Template Method for damage handling.
    /// </summary>
    [RequireComponent(typeof(IDamageable))]
    public abstract class BaseDamageController : MonoBehaviour
    {
        protected IDamageable Damageable;

        protected virtual void Awake()
        {
            Damageable = GetComponent<IDamageable>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            TryDamageDealerTemplate(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamageDealerTemplate(other.gameObject);
        }

        private void TryDamageDealerTemplate(GameObject other)
        {
            if (Damageable == null)
                return;
            if (!other.TryGetComponent(out IDamageDealer dealer))
                return;
            if (!ShouldProcessDealer(dealer))
                return;
            ProcessDamage(dealer);
        }

        /// <summary>
        /// Hook: decide whether to process damage from this dealer.
        /// </summary>
        protected abstract bool ShouldProcessDealer(IDamageDealer dealer);

        /// <summary>
        /// Hook: apply damage after passing checks.
        /// </summary>
        protected abstract void ProcessDamage(IDamageDealer dealer);
    }

}
