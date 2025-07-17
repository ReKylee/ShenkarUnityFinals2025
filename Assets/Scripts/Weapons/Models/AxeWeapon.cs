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
        private IGameDataService _gameDataService;
        private bool _isUnlocked;
        private bool _isEquipped;

        #region VContainer Injection
        [Inject]
        public void Construct(IGameDataService gameDataService)
        {
            Debug.Log("AxeWeapon: VContainer injection successful!");
            _gameDataService = gameDataService;
        }
        #endregion

        private void Start()
        {
            Debug.Log($"AxeWeapon: Start - GameDataService is {(_gameDataService != null ? "available" : "NULL")}");
            UpdateUnlockStatus();
            // AxeWeapon starts equipped when unlocked
            if (_isUnlocked)
            {
                Equip();
            }
        }

        private void UpdateUnlockStatus()
        {
            bool wasUnlocked = _isUnlocked;
            _isUnlocked = _gameDataService?.HasPowerUp("axe") == true;
            Debug.Log($"AxeWeapon: UpdateUnlockStatus - GameDataService: {_gameDataService != null}, HasPowerUp result: {_isUnlocked}");
            
            if (wasUnlocked != _isUnlocked)
            {
                Debug.Log($"AxeWeapon: Unlock status changed from {wasUnlocked} to {_isUnlocked}");
            }
        }

        public bool IsUnlocked => _isUnlocked;
        public bool IsEquipped => _isEquipped;

        public void Shoot()
        {
            Debug.Log($"AxeWeapon: Shoot called - Unlocked: {_isUnlocked}, Equipped: {_isEquipped}, Cooldown ready: {Time.time >= _nextFireTime}");
            
            // Check if power-up is unlocked first
            if (!_isUnlocked)
            {
                UpdateUnlockStatus(); // Check again in case it was unlocked during gameplay
                if (!_isUnlocked) 
                {
                    Debug.Log("AxeWeapon: Cannot shoot - weapon not unlocked");
                    return;
                }
            }

            // Check if weapon is equipped
            if (!_isEquipped)
            {
                Debug.Log("AxeWeapon: Cannot shoot - weapon not equipped");
                return;
            }

            // Check cooldown
            if (Time.time < _nextFireTime)
            {
                Debug.Log("AxeWeapon: Cannot shoot - still on cooldown");
                return;
            }

            // Check axe prefab
            if (!axe)
            {
                Debug.Log($"AxeWeapon: Cannot shoot - Axe prefab is null");
                return;
            }

            Debug.Log("AxeWeapon: Firing axe!");
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
            if (!_isUnlocked)
            {
                UpdateUnlockStatus();
                if (!_isUnlocked) return;
            }
            
            _isEquipped = true;
            Debug.Log("AxeWeapon: Equipped");
        }

        public void UnEquip()
        {
            _isEquipped = false;
            Debug.Log("AxeWeapon: Unequipped");
        }

        // Public method for WeaponController to check if weapon should be available
        public void RefreshUnlockStatus()
        {
            UpdateUnlockStatus();
            
            // Auto-equip when unlocked
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
