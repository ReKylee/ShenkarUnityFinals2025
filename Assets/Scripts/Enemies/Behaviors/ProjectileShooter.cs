using System;
using UnityEngine;
using Enemies.Interfaces;
using Weapons.Models;

namespace Enemies.Behaviors
{
    // Shoots a projectile at intervals from a specified fire point
    public class ProjectileShooter : MonoBehaviour, IAttackBehavior
    {
        [SerializeField] private FireballWeapon fireballWeapon;
        [SerializeField] private float fireInterval = 2f;

        private float _lastFireTime;
        private void Start()
        {
            fireballWeapon.Equip();
        }
        public void Attack()
        {
            if (!fireballWeapon)
                return;
            if (Time.time - _lastFireTime < fireInterval)
                return;
            fireballWeapon.Shoot();
            _lastFireTime = Time.time;
        }

  
    }
}
