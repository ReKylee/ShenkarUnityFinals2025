using PowerUps._Base;
using UnityEngine;
using Weapons.Services;

namespace PowerUps.FireFlower
{
    public class FireFlowerPowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            Debug.Log("ApplyPowerUp Fire Flower - Switching to temporary Fireball weapon");
            WeaponManagerService weaponManager = player?.GetComponentInChildren<WeaponManagerService>();
            weaponManager?.SwitchToTemporaryWeapon();
        }
    }
}
