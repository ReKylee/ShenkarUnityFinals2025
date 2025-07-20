using PowerUps._Base;
using UnityEngine;
using Weapons;
using Weapons.Services;

namespace PowerUps.Boomerang
{
    public class PickableBoomerangPowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            WeaponManagerService weaponManager = player?.GetComponentInChildren<WeaponManagerService>();
            weaponManager?.SwitchToPrimaryWeapon(WeaponType.Boomerang);
        }
    }
}
