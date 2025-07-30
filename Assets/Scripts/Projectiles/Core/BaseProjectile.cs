using Pooling;
using UnityEngine;
using Weapons;
using Weapons.Interfaces;

namespace Projectiles.Core
{
    public abstract class BaseProjectile : MonoBehaviour, IWeaponTypeProvider, IPoolable
    {
        [SerializeField] protected Vector2 speed = new(12f, 0f);

        // Direct references for pooling - exposed through IPoolable interface
        private IPoolService _poolService;
        private GameObject _sourcePrefab;

        protected Rigidbody2D Rb;

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        public void SetPoolingInfo(IPoolService poolService, GameObject sourcePrefab)
        {
            _poolService = poolService;
            _sourcePrefab = sourcePrefab;
        }

        public void ReturnToPool()
        {
            if (_poolService != null && _sourcePrefab)
            {
                _poolService.Release(_sourcePrefab, gameObject);
            }

        }
        public virtual WeaponType WeaponType { get; set; }

        public void Fire()
        {
            Move();
        }

        protected abstract void Move();

        protected void DestroyProjectile()
        {
            if (gameObject.activeInHierarchy)
            {
                // Reset all physics properties
                Rb.linearVelocity = Vector2.zero;
                Rb.angularVelocity = 0f;

                // Reset any accumulated forces
                Rb.totalForce = Vector2.zero;
                Rb.totalTorque = 0f;

                // Return to pool
                ReturnToPool();
            }
        }
    }
}
