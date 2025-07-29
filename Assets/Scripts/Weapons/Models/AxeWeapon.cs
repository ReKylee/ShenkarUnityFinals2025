using System;
using Pooling;
using Projectiles;
using UnityEngine;
using VContainer;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class AxeWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Axe;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.5f;
        [SerializeField] private GameObject axePrefab;
        private float _nextFireTime;

        private IPoolService _poolService;

        [NonSerialized] private Rigidbody2D _throwerRb;
        private bool IsEquipped { get; set; }

        private void Awake()
        {
            _throwerRb = GetComponentInParent<Rigidbody2D>();
        }
     
        public WeaponType WeaponType => weaponType;
        public void Shoot()
        {
            if (!IsEquipped)
            {
                return;
            }

            if (Time.time < _nextFireTime)
            {
                return;
            }

            ProjectileAxe scAxe = _poolService?.Get<ProjectileAxe>(axePrefab,
                spawnPoint ? spawnPoint.position : transform.position, Quaternion.identity);

            if (scAxe)
            {
                scAxe.SetPoolingInfo(_poolService, axePrefab);

                scAxe.gameObject.layer = gameObject.layer;
                scAxe.Direction = transform.parent?.localScale.x ?? 1;
                scAxe.ThrowerVelocityX = _throwerRb.linearVelocity.x;
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

        [Inject]
        private void Configure(IPoolService poolService)
        {
            _poolService = poolService;
        }
    }
}
