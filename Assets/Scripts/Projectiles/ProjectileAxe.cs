using System;
using System.Collections;
using GabrielBigardi.SpriteAnimator;
using Projectiles.Core;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileAxe : BaseProjectile
    {
        [SerializeField] private float explodeTime = 1f;
        [SerializeField] private SpriteAnimator explosionAnimator;
        private bool _exploding;
        [NonSerialized] public float Direction;

        private void OnBecameInvisible()
        {
            ReturnToPool();
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
                return;

            Debug.Log($"Axe hit {other.gameObject.name}.");
            if (!_exploding)
                StartCoroutine(Explode());

        }

        private IEnumerator Explode()
        {
            _exploding = true;
            yield return new WaitForSeconds(explodeTime);
            explosionAnimator.gameObject.SetActive(true);
            yield return new WaitUntil(() => explosionAnimator.AnimationCompleted);
            explosionAnimator.gameObject.SetActive(false);
            _exploding = false;
            ReturnToPool();
        }
        protected override void Move()
        {
            transform.localScale = new Vector3(Direction, 1, 1);
            Rb.AddForce(new Vector2(speed.x * Direction, speed.y), ForceMode2D.Impulse);
        }
    }
}
