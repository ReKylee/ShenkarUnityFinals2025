using System;
using Core.Data;
using Projectiles;
using UnityEngine;
using VContainer;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class BoomerangWeapon : MonoBehaviour, IAmmoWeapon
    {
        [SerializeField] private GameObject boomerang;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;
        
        private float _nextFireTime;
        private IGameDataService _gameDataService;
        private bool _isUnlocked;
        private Transform _playerTransform;
        private GameObject _activeBoomerang; // Track the single active boomerang
        
        // Boomerang always has max 1 ammo
        public int CurrentAmmo { get; private set; } = 1;
        public int MaxAmmo => 1;
        public bool HasAmmo => CurrentAmmo > 0;
        public bool IsUnlocked => _isUnlocked;

        public event Action<int> OnAmmoChanged;

        #region VContainer Injection
        [Inject]
        public void Construct(IGameDataService gameDataService)
        {
            Debug.Log("BoomerangWeapon: VContainer injection successful!");
            _gameDataService = gameDataService;
        }
        #endregion

        private void Start()
        {
            Debug.Log($"BoomerangWeapon: Start - GameDataService is {(_gameDataService != null ? "available" : "NULL")}");
            UpdateUnlockStatus();
            
            // Find player transform for boomerang returning
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void UpdateUnlockStatus()
        {
            bool wasUnlocked = _isUnlocked;
            _isUnlocked = _gameDataService?.HasPowerUp("boomerang") == true;
            Debug.Log($"BoomerangWeapon: UpdateUnlockStatus - GameDataService: {_gameDataService != null}, HasPowerUp result: {_isUnlocked}");
            
            if (wasUnlocked != _isUnlocked)
            {
                Debug.Log($"BoomerangWeapon: Unlock status changed from {wasUnlocked} to {_isUnlocked}");
            }
        }

        public void SetAmmo(int ammo)
        {
            int oldAmmo = CurrentAmmo;
            CurrentAmmo = Mathf.Clamp(ammo, 0, MaxAmmo);

            if (oldAmmo != CurrentAmmo)
            {
                OnAmmoChanged?.Invoke(CurrentAmmo);
                Debug.Log($"BoomerangWeapon: Ammo changed from {oldAmmo} to {CurrentAmmo}");
            }
        }

        public void Shoot()
        {
            Debug.Log($"BoomerangWeapon: Shoot called - Unlocked: {_isUnlocked}, HasAmmo: {HasAmmo}, Cooldown ready: {Time.time >= _nextFireTime}");
            
            // Check if power-up is unlocked first
            if (!_isUnlocked)
            {
                UpdateUnlockStatus(); // Check again in case it was unlocked during gameplay
                if (!_isUnlocked) 
                {
                    Debug.Log("BoomerangWeapon: Cannot shoot - weapon not unlocked");
                    return;
                }
            }

            // Check cooldown
            if (Time.time < _nextFireTime)
            {
                Debug.Log("BoomerangWeapon: Cannot shoot - still on cooldown");
                return;
            }

            // Check ammo and boomerang prefab
            if (!boomerang || !HasAmmo)
            {
                Debug.Log($"BoomerangWeapon: Cannot shoot - Boomerang prefab: {boomerang != null}, HasAmmo: {HasAmmo}");
                return;
            }

            // Check if there's already an active boomerang
            if (_activeBoomerang != null)
            {
                Debug.Log("BoomerangWeapon: Cannot shoot - boomerang already active");
                return;
            }

            Debug.Log("BoomerangWeapon: Firing boomerang!");
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            _activeBoomerang = Instantiate(boomerang, spawnPosition, Quaternion.identity);

            if (_activeBoomerang.TryGetComponent(out ProjectileBoomerang scBoomerang))
            {
                _activeBoomerang.layer = gameObject.layer;

                // Consume ammo
                SetAmmo(0);

                // Set up boomerang properties
                float direction = transform.parent?.localScale.x ?? 1;
                scBoomerang.Direction = direction;
                scBoomerang.PlayerTransform = _playerTransform;
                
                // Subscribe to return event to restore ammo
                scBoomerang.OnBoomerangReturned += OnBoomerangReturned;
                
                scBoomerang.Fire();

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        private void OnBoomerangReturned()
        {
            Debug.Log("BoomerangWeapon: Boomerang returned, restoring ammo");
            
            // Clean up the active boomerang reference
            if (_activeBoomerang != null)
            {
                // Unsubscribe from events before destroying
                if (_activeBoomerang.TryGetComponent(out ProjectileBoomerang scBoomerang))
                {
                    scBoomerang.OnBoomerangReturned -= OnBoomerangReturned;
                }
                
                Destroy(_activeBoomerang);
                _activeBoomerang = null;
            }
            
            SetAmmo(1); // Restore the single ammo when boomerang returns
        }

        public void Reload()
        {
            // Boomerang doesn't use traditional reload - ammo is restored when it returns
            // But we can implement this for manual reload if needed
            SetAmmo(1);
        }

        // Public method for WeaponController to check if weapon should be available
        public void RefreshUnlockStatus()
        {
            UpdateUnlockStatus();
        }

        private void OnDestroy()
        {
            // Clean up active boomerang if weapon is destroyed
            if (_activeBoomerang != null)
            {
                if (_activeBoomerang.TryGetComponent(out ProjectileBoomerang scBoomerang))
                {
                    scBoomerang.OnBoomerangReturned -= OnBoomerangReturned;
                }
                Destroy(_activeBoomerang);
            }
        }
    }
}
