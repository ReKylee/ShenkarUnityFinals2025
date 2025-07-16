using PowerUps._Base;
using UnityEngine;
using Weapons.Models;

namespace PowerUps.LaserPowerUp
{
    public class LaserPowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            Debug.Log("ApplyPowerUp Fire Flower");
            LaserWeapon laserWeapon = player?.GetComponentInChildren<LaserWeapon>();
            laserWeapon?.Equip();
        }
    }
}
