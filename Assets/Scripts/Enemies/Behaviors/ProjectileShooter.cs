using Enemies.Interfaces;
using UnityEngine;
using Weapons.Models;

namespace Enemies.Behaviors
{
    // Command to shoot a projectile at intervals
    public class ProjectileShootCommand : MonoBehaviour, IAttackCommand
    {
        [SerializeField] private FireballWeapon fireballWeapon;
        [SerializeField] private float fireInterval = 2f;

        private float _lastFireTime;

        private void Start()
        {
            fireballWeapon.Equip();
        }

        public void Execute()
        {
            if (!fireballWeapon)
                return;

            if (Time.time - _lastFireTime < fireInterval)
                return;

            fireballWeapon.Shoot();
            _lastFireTime = Time.time;
        }

        public void ResetCooldown()
        {
            _lastFireTime = 0f;
        }
    }
}
