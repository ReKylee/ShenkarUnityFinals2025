using PowerUps._Base;
using UnityEngine;
using Weapons.Services;

namespace PowerUps.Boomerang
{
    public class PickableBoomerangPowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            Debug.Log("ApplyPowerUp Boomerang - Switching to Boomerang weapon");
            WeaponManagerService weaponManager = player?.GetComponentInChildren<WeaponManagerService>();
            weaponManager?.SwitchToPrimaryWeapon(WeaponType.Boomerang);
        }
    }
}
