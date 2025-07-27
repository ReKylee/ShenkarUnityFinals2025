using Health.Interfaces;
using UnityEngine;

namespace Player.Components
{
    /// <summary>
    /// When enabled, instantly kills any damageable object the player collides with.
    /// Should be enabled only when the player is invincible.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInvincibleDamageDealer : MonoBehaviour, IDamageDealer
    {
        [SerializeField] private int damageAmount = 9999; // "Kill" value
        [SerializeField] private LayerMask targetLayers = ~0; // All by default

        public int GetDamageAmount() => damageAmount;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & targetLayers) == 0)
                return;

            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
                damageable?.Damage(damageAmount, gameObject);
        }
    }
}
