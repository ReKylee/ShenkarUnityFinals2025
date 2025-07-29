using Pooling;
using Projectiles;
using UnityEngine;
using VContainer;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class FireballWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Fireball;

        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;
        [SerializeField] private GameObject fireballPrefab;

        private float _nextFireTime;

        private IPoolService _poolService;

        private bool IsEquipped { get; set; }
        public WeaponType WeaponType => weaponType;

        public void Shoot()
        {
            if (!IsEquipped)
                return;

            if (Time.time < _nextFireTime)
                return;

            ProjectileFireball scFireball = _poolService?.Get<ProjectileFireball>(fireballPrefab,
                spawnPoint ? spawnPoint.position : transform.position, Quaternion.identity);

            if (scFireball)
            {
                scFireball.SetPoolingInfo(_poolService, fireballPrefab);

                scFireball.gameObject.layer = gameObject.layer;
                scFireball.Direction = transform.parent?.localScale.x ?? 1;
                scFireball.WeaponType = weaponType;
                scFireball.Fire();

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
