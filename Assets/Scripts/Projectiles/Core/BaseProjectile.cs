using System;
using UnityEngine;
using Weapons;
using Weapons.Interfaces;

namespace Projectiles.Core
{
    public abstract class BaseProjectile : MonoBehaviour, IWeaponTypeProvider
    {
        [SerializeField] protected Vector2 speed = new(12f, 0f);

        protected Rigidbody2D Rb;
        private bool _isDestroyed;
        public Action<GameObject> OnProjectileDestroyed { get; set; }

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
        }
        public virtual WeaponType WeaponType { get; set; }
        private void OnEnable()
        {
            _isDestroyed = false;
        }

        public void Fire()
        {
            Move();
        }

        protected abstract void Move();

        protected void DestroyProjectile()
        {
            if (_isDestroyed)
                return;
            if (!gameObject.activeInHierarchy)
                return;

            _isDestroyed = true;

            // Reset all physics properties
            Rb.linearVelocity = Vector2.zero;
            Rb.angularVelocity = 0f;

            // Reset any accumulated forces
            Rb.totalForce = Vector2.zero;
            Rb.totalTorque = 0f;
            OnProjectileDestroyed?.Invoke(gameObject);
        }
    }
}
