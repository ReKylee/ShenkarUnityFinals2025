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

        private void OnBecameInvisible()
        {
            DestroyProjectile();
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            DestroyProjectile();
        }
        private IEnumerator DestroyObject()
        {
            yield return new WaitForSeconds(destroyTime);
            DestroyProjectile();
        }


        protected override void Move()
        {
            Rb.linearVelocity = new Vector2(speed.x * Direction, speed.y);
            StartCoroutine(DestroyObject());
        }
    }
}
