using PowerUps._Base;
using UnityEngine;
using Weapons.Models;

namespace PowerUps.FireFlower
{
    public class FireFlowerPowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            Debug.Log("ApplyPowerUp Fire Flower");
            FireballWeapon fireballWeapon = player?.GetComponentInChildren<FireballWeapon>();
            fireballWeapon?.Equip();
        }
    }
}
