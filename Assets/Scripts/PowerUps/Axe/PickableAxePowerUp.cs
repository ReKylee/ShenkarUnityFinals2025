using PowerUps._Base;
using UnityEngine;
using Weapons;
using Weapons.Services;

namespace PowerUps.Axe
{
    public class PickableAxePowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            WeaponManagerService weaponManager = player?.GetComponentInChildren<WeaponManagerService>();
            weaponManager?.SwitchToPrimaryWeapon(WeaponType.Axe);
        }
    }
}
