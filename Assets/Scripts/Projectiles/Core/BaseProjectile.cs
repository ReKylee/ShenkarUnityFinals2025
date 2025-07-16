using UnityEngine;
using UnityEngine.Pool;

namespace Projectiles.Core
{
    public abstract class BaseProjectile : MonoBehaviour
    {
        [SerializeField] protected Vector2 speed = new(12f, 0f);

        protected Rigidbody2D Rb;
        public IObjectPool<GameObject> Pool { get; set; }

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
        }


        public void Fire()
        {
            Debug.Log("BaseProjectile: Firing projectile.");
            Move();
        }


        protected abstract void Move();

        protected void ReturnToPool()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (Pool != null)
            {
                Debug.Log($"Projectile '{gameObject.name}' returning to pool.");
                Rb.linearVelocity = Vector2.zero;

                Pool.Release(gameObject);
            }
            else
            {
                Debug.LogWarning(
                    $"Projectile '{gameObject.name}' does not have a pool to return to. Destroying instead.");

                DestroyImmediate(gameObject);
            }
        }
    }
}
