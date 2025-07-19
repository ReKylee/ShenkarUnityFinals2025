using PowerUps._Base;
using UnityEngine;
using Weapons.Services;

namespace PowerUps.FireFlower
{
    public class FireFlowerPowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            WeaponManagerService weaponManager = player?.GetComponentInChildren<WeaponManagerService>();
            weaponManager?.SwitchToTemporaryWeapon();
        }
    }
}
