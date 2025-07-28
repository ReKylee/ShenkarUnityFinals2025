using System;
using Projectiles.Core;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileAxe : BaseProjectile
    {
        [NonSerialized] public float Direction;
        [NonSerialized] public float ThrowerVelocityX;
        private void OnBecameInvisible()
        {
            DestroyProjectile();
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
                return;

            DestroyProjectile();
            Debug.Log($"Axe hit {other.gameObject.name}.");
        }


        protected override void Move()
        {

            // World-space velocity = player's current velocity + desired arc velocity
            Vector2 worldVelocity = new(speed.x * Direction + ThrowerVelocityX, speed.y);

            // Cancel out thrower motion to isolate projectile speed
            Rb.linearVelocity = worldVelocity;
        }
    }
}
