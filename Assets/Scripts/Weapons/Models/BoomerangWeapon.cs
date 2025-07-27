using System;
using Projectiles;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class BoomerangWeapon : MonoBehaviour, IAmmoWeapon
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Boomerang;
        public WeaponType WeaponType => weaponType;        
        [SerializeField] private GameObject boomerang;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;

        private float _nextFireTime;

        private GameObject _pooledBoomerang;
        private ProjectileBoomerang _pooledProjectile;
        private Transform _returnToTransform;

        private void Start()
        {
            _returnToTransform = transform.parent;

            if (!boomerang)
                return;

            Transform spawnParent = spawnPoint ? spawnPoint : transform;
            _pooledBoomerang = Instantiate(boomerang, spawnParent.position, Quaternion.identity, spawnParent);
            _pooledProjectile = _pooledBoomerang.GetComponent<ProjectileBoomerang>();
            _pooledProjectile.WeaponType = WeaponType;
            
            _pooledBoomerang.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_pooledProjectile)
                _pooledProjectile.OnBoomerangReturned -= OnBoomerangReturned;

            if (_pooledBoomerang)
                Destroy(_pooledBoomerang);
        }

        public int CurrentAmmo { get; private set; } = 1;
        public int MaxAmmo => 1;
        public bool HasAmmo => CurrentAmmo > 0;

        public void SetAmmo(int ammo)
        {
            int old = CurrentAmmo;
            CurrentAmmo = Mathf.Clamp(ammo, 0, MaxAmmo);
            if (old != CurrentAmmo)
                OnAmmoChanged?.Invoke(CurrentAmmo);
        }

        public void Shoot()
        {
            if (Time.time < _nextFireTime || !HasAmmo || !_pooledBoomerang || _pooledBoomerang.activeSelf)
                return;

            _pooledBoomerang.transform.SetPositionAndRotation(
                spawnPoint ? spawnPoint.position : transform.position,
                Quaternion.identity
            );

            _pooledBoomerang.layer = gameObject.layer;
            SetAmmo(0);

            float dir = transform.parent?.localScale.x ?? 1;
            _pooledProjectile.Direction = dir;
            _pooledProjectile.PlayerTransform = _returnToTransform;
            _pooledProjectile.OnBoomerangReturned += OnBoomerangReturned;

            _pooledBoomerang.SetActive(true);
            _pooledProjectile.Fire();

            _nextFireTime = Time.time + cooldownTime;
        }

        public void Reload()
        {
            SetAmmo(1);
        }
        public event Action<int> OnAmmoChanged;

        private void OnBoomerangReturned()
        {
            _pooledProjectile.OnBoomerangReturned -= OnBoomerangReturned;
            _pooledBoomerang.SetActive(false);
            SetAmmo(1);
        }
    }
}
