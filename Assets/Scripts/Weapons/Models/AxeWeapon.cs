using System;
using Projectiles;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class AxeWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.5f;
        [SerializeField] private AxePool axePool;
        private float _nextFireTime;

        [NonSerialized] private Rigidbody2D _throwerRb;

        public bool IsEquipped { get; private set; }

        private void Awake()
        {
            _throwerRb = GetComponentInParent<Rigidbody2D>();
        }
        public WeaponType WeaponType => WeaponType.Axe;
        public void Shoot()
        {
            // Check if weapon is equipped
            if (!IsEquipped)
            {
                return;
            }

            // Check cooldown
            if (Time.time < _nextFireTime)
            {
                return;
            }

            GameObject curAxe = axePool.Get();
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            curAxe.transform.position = spawnPosition;
            curAxe.transform.rotation = Quaternion.identity;

            if (curAxe.TryGetComponent(out ProjectileAxe scAxe))
            {
                curAxe.layer = gameObject.layer;

                scAxe.Direction = transform.parent?.localScale.x ?? 1;
                scAxe.ThrowerVelocityX = _throwerRb.linearVelocityX;

                scAxe.Fire();

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        public void Equip()
        {
            IsEquipped = true;
        }

        public void UnEquip()
        {
            IsEquipped = false;
        }
    }
}
