using System;
using Core.Data;
using Projectiles;
using UnityEngine;
using VContainer;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class AxeWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private GameObject axe;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.5f;
        [SerializeField] private AxePool axePool;
        
        private float _nextFireTime;
        private bool _isEquipped;

        public bool IsEquipped => _isEquipped;

        public void Shoot()
        {
            // Check if weapon is equipped
            if (!_isEquipped)
            {
                return;
            }

            // Check cooldown
            if (Time.time < _nextFireTime)
            {
                return;
            }

            // Check axe prefab
            if (!axe)
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

                float direction = transform.parent?.localScale.x ?? 1;
                scAxe.Direction = direction;
                scAxe.Fire();

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        public void Equip()
        {
            _isEquipped = true;
        }

        public void UnEquip()
        {
            _isEquipped = false;
        }
    }
}
