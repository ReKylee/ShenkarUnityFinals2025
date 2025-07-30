using Enemies.Interfaces;
using UnityEngine;
using Weapons.Models;

namespace Enemies.Behaviors
{
    // Command to shoot a projectile at intervals
    public class ProjectileShootCommand : MonoBehaviour, IAttackCommand
    {
        [SerializeField] private FireballWeapon fireballWeapon;

        private void Start()
        {
            fireballWeapon.Equip();
        }

        public void Execute()
        {
            if (!fireballWeapon)
                return;

            fireballWeapon.Shoot();

        }

    }
}
