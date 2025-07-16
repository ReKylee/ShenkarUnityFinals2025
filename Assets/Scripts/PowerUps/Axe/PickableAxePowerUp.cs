using PowerUps._Base;
using UnityEngine;
using Weapons.Models;

namespace PowerUps.Axe
{
    public class PickableAxePowerUp : IPowerUp
    {
        public void ApplyPowerUp(GameObject player)
        {
            Debug.Log("ApplyPowerUp Fire Flower");
            AxeWeapon axeWeapon = player?.GetComponentInChildren<AxeWeapon>();
            axeWeapon?.Reload();
        }
    }
}
