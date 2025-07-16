using Projectiles;
using Resettables;
using UnityEngine;
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
        private UsableWeaponResetter _resetter;
        private void Start()
        {
            _resetter = new UsableWeaponResetter(this);
        }

        public void Shoot()
        {
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
