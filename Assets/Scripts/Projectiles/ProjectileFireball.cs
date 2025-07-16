using System;
using System.Collections;
using Projectiles.Core;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileFireball : BaseProjectile
    {
        public float destroyTime = 5f;
        [NonSerialized] public float Direction;
        private void OnEnable()
        {
            StartCoroutine(DestroyObject());
        }
        private void OnBecameInvisible()
        {
            ReturnToPool();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                return;

            Debug.Log($"Laser hit {other.gameObject.name}.");
            ReturnToPool();
        }
        private IEnumerator DestroyObject()
        {
            yield return new WaitForSeconds(destroyTime);
            ReturnToPool();
        }


        protected override void Move()
        {
            Rb.linearVelocity = new Vector2(speed.x * Direction, speed.y);
        }
    }
}
