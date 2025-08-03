using Health.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    public class EnemyDeathEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer deathEffectPrefab;

        [Header("Launch Parameters")] [SerializeField]
        private Vector2 launchVelocity = new Vector2(3f, 8f);

        [SerializeField] private float gravityScale = 2f;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            IHealthEvents healthEvents = GetComponent<IHealthEvents>();
            if (healthEvents != null)
                healthEvents.OnDeath += PlayDeathEffect;
        }

        private void OnDestroy()
        {
            IHealthEvents healthEvents = GetComponent<IHealthEvents>();
            if (healthEvents != null)
                healthEvents.OnDeath -= PlayDeathEffect;
        }

        private void PlayDeathEffect()
        {
            if (!deathEffectPrefab)
                return;

            SpriteRenderer effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            effect.sprite = _spriteRenderer.sprite;
            effect.transform.localScale = transform.localScale;
            effect.flipY = true;
            
            if (effect.TryGetComponent(out Rigidbody2D rb))
            {
                rb.gravityScale = gravityScale;
                
                Vector2 adjustedVelocity = launchVelocity;
                adjustedVelocity.x *= Mathf.Sign(transform.localScale.x);
                
                rb.linearVelocity = adjustedVelocity;
            }
        }
    }
}
