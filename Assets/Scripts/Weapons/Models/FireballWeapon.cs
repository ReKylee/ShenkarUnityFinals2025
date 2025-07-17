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
        
        private bool _isEquipped;
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
            // Check if fireball power-up is unlocked
            if (!_gameDataService?.HasPowerUp("fireball") == true)
            {
                gameObject.SetActive(false);
            }
        }

        public void Shoot()
        {
            // Check if power-up is unlocked
            if (!_gameDataService?.HasPowerUp("fireball") == true)
                return;

            // Check cooldown
            if (Time.time < _nextFireTime)
                return;

            if (!fireball || !_isEquipped)
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
            _isEquipped = true;
        }

        public void UnEquip()
        {
            _isEquipped = false;
        }
    }
}
