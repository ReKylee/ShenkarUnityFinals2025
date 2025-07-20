﻿using System;
using Projectiles;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class BoomerangWeapon : MonoBehaviour, IAmmoWeapon
    {
        [SerializeField] private GameObject boomerang;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;
        
        private float _nextFireTime;
        private Transform _returnToTransform;
        private GameObject _activeBoomerang; 
        
        // Boomerang always has max 1 ammo
        public int CurrentAmmo { get; private set; } = 1;
        public int MaxAmmo => 1;
        public bool HasAmmo => CurrentAmmo > 0;

        public event Action<int> OnAmmoChanged;

        private void Start()
        {
            _returnToTransform = transform.parent;
        }

        public void SetAmmo(int ammo)
        {
            int oldAmmo = CurrentAmmo;
            CurrentAmmo = Mathf.Clamp(ammo, 0, MaxAmmo);

            if (oldAmmo != CurrentAmmo)
            {
                OnAmmoChanged?.Invoke(CurrentAmmo);
            }
        }

        public void Shoot()
        {
            // Check cooldown
            if (Time.time < _nextFireTime)
            {
                return;
            }

            // Check ammo and boomerang prefab
            if (!boomerang || !HasAmmo)
            {
                return;
            }

            // Check if there's already an active boomerang
            if (_activeBoomerang != null)
            {
                return;
            }

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
                scBoomerang.PlayerTransform = _returnToTransform;
                
                // Subscribe to return event to restore ammo
                scBoomerang.OnBoomerangReturned += OnBoomerangReturned;
                
                scBoomerang.Fire();

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        private void OnBoomerangReturned()
        {
            // Clean up the active boomerang reference
            if (_activeBoomerang)
            {
                // Unsubscribe from events before destroying
                if (_activeBoomerang.TryGetComponent(out ProjectileBoomerang scBoomerang))
                {
                    scBoomerang.OnBoomerangReturned -= OnBoomerangReturned;
                }
                
                Destroy(_activeBoomerang);
                _activeBoomerang = null;
            }
            
            SetAmmo(1); 
        }

        public void Reload()
        {
            // Boomerang doesn't use traditional reload - ammo is restored when it returns
            // But we can implement this for manual reload if needed
            SetAmmo(1);
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
