using Projectiles.Core;
using UnityEngine;

namespace Projectiles.OverEngineeredLaser
{

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileLaser : BaseProjectile
    {

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

        protected override void Move()
        {
            Rb.linearVelocity = Vector2.up * speed;
        }


        public void SetSpeed(Vector2 newSpeed)
        {
            speed = newSpeed;
        }
    }
}
