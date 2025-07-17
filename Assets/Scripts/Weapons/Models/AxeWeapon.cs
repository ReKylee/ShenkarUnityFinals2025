using System;
using Core;
using Projectiles;
using UnityEngine;
using VContainer;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class AxeWeapon : MonoBehaviour, IAmmoWeapon
    {
        [SerializeField] private GameObject axe;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int maxAmmo = 3;
        [SerializeField] private float cooldownTime = 0.5f;
        [SerializeField] private int defaultAmmo;
        [SerializeField] private AxePool axePool;
        
        private float _nextFireTime;
        private PersistentDataManager _persistentData;

        #region VContainer Injection
        [Inject]
        public void Construct(PersistentDataManager persistentData)
        {
            _persistentData = persistentData;
        }
        #endregion

        private void Awake()
        {
            CurrentAmmo = defaultAmmo;
        }

        private void Start()
        {
            // Check if axe power-up is unlocked
            if (!_persistentData?.HasPowerUp("axe") == true)
            {
                gameObject.SetActive(false);
            }
        }

        public int CurrentAmmo { get; private set; }
        public int MaxAmmo => maxAmmo;

        // Check if weapon has ammo
        public bool HasAmmo => CurrentAmmo > 0;

        public void SetAmmo(int ammo)
        {
            int oldAmmo = CurrentAmmo;
            CurrentAmmo = Mathf.Clamp(ammo, 0, maxAmmo);

            if (oldAmmo != CurrentAmmo)
            {
                OnAmmoChanged?.Invoke(CurrentAmmo);
            }
        }

        public void Shoot()
        {
            // Check if power-up is unlocked
            if (!_persistentData?.HasPowerUp("axe") == true)
                return;

            // Check cooldown
            if (Time.time < _nextFireTime)
                return;

            if (!axe || !HasAmmo)
                return;

            GameObject curAxe = axePool.Get();
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            curAxe.transform.position = spawnPosition;
            curAxe.transform.rotation = Quaternion.identity;

            if (curAxe.TryGetComponent(out ProjectileAxe scAxe))
            {
                curAxe.layer = gameObject.layer;

                // Reduce ammo and notify listeners
                SetAmmo(CurrentAmmo - 1);

                float direction = transform.parent?.localScale.x ?? 1;
                scAxe.Direction = direction;
                scAxe.Fire();

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        // IAmmoWeapon implementation - Reload now adds ammo in specific increments
        public void Reload()
        {
            // Add one ammo but don't exceed max
            SetAmmo(CurrentAmmo + 1);
        }

        // Events for ammo changes
        public event Action<int> OnAmmoChanged;
    }
}
