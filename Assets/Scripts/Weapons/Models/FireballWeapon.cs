using Projectiles;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class FireballWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Fireball;

        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;
        [SerializeField] private FireballPool fireballPool;

        private float _nextFireTime;

        public bool IsEquipped { get; private set; }
        public WeaponType WeaponType => weaponType;

        public void Shoot()
        {
            // Check if weapon is equipped
            if (!IsEquipped)
                return;

            // Check cooldown
            if (Time.time < _nextFireTime)
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
            IsEquipped = true;
        }

        public void UnEquip()
        {
            IsEquipped = false;
        }
    }
}
