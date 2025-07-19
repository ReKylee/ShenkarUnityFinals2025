using System;
using Projectiles.Core;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileAxe : BaseProjectile
    {
        [NonSerialized] public float Direction;

        private void OnBecameInvisible()
        {
            ReturnToPool();
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
                return;

            ReturnToPool();
            Debug.Log($"Axe hit {other.gameObject.name}.");
        }


        protected override void Move()
        {
            transform.localScale = new Vector3(Direction, 1, 1);
            Rb.AddForce(new Vector2(speed.x * Direction, speed.y), ForceMode2D.Impulse);
        }
    }
}
