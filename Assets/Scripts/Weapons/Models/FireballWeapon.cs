using System;
using Core.Data;
using Projectiles;
using UnityEngine;
using VContainer;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class FireballWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private GameObject fireball;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;
        [SerializeField] private FireballPool fireballPool;
        
        private bool _isEquipped = false;
        private bool _isUnlocked = false;
        private float _nextFireTime;
        private IGameDataService _gameDataService;

        #region VContainer Injection
        [Inject]
        public void Construct(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }
        #endregion

        private void Start()
        {
            UpdateUnlockStatus();
            // FireballWeapon typically starts equipped when unlocked
            if (_isUnlocked)
            {
                Equip();
            }
        }

        private void UpdateUnlockStatus()
        {
            _isUnlocked = _gameDataService?.HasPowerUp("fireball") == true;
            
            // Don't disable the GameObject here - let WeaponController handle visibility
            // The weapon should exist but not be usable until unlocked
        }

        public bool IsUnlocked => _isUnlocked;
        public bool IsEquipped => _isEquipped;

        public void Shoot()
        {
            // Check if power-up is unlocked first
            if (!_isUnlocked)
            {
                UpdateUnlockStatus(); // Check again in case it was unlocked during gameplay
                if (!_isUnlocked) return;
            }

            // Check if weapon is equipped
            if (!_isEquipped)
                return;

            // Check cooldown
            if (Time.time < _nextFireTime)
                return;

            // Check fireball prefab
            if (!fireball)
                return;

            GameObject curFireball = fireballPool.Get();
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            curFireball.transform.position = spawnPosition;
            curFireball.transform.rotation = Quaternion.identity;

            if (curFireball.TryGetComponent(out ProjectileFireball scFireball))
            {
                curFireball.layer = gameObject.layer;
                float direction = transform.parent?.localScale.x ?? 1;
                scFireball.Direction = direction;
                scFireball.Fire();

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        public void Equip()
        {
            if (!_isUnlocked)
            {
                UpdateUnlockStatus();
                if (!_isUnlocked) return;
            }
            
            _isEquipped = true;
        }

        public void UnEquip()
        {
            _isEquipped = false;
        }

        // Public method for WeaponController to check if weapon should be available
        public void RefreshUnlockStatus()
        {
            UpdateUnlockStatus();
            
            // Auto-equip when unlocked (fireball is typically always equipped when available)
            if (_isUnlocked && !_isEquipped)
            {
                Equip();
            }
            else if (!_isUnlocked && _isEquipped)
            {
                UnEquip();
            }
        }
    }
}
