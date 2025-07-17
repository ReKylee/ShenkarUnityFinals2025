using PowerUps._Base;
using UnityEngine;
using Weapons.Services;

namespace PowerUps.Axe
{
    public class PickableAxePowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            Debug.Log("ApplyPowerUp Axe - Switching to Axe weapon");
            WeaponManagerService weaponManager = player?.GetComponentInChildren<WeaponManagerService>();
            weaponManager?.SwitchToPrimaryWeapon(WeaponType.Axe);
        }
    }
}
