using PowerUps._Base;
using UnityEngine;

namespace PowerUps.HealthUp
{
    public class HealthCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private int healAmount = 1;

        protected override IPowerUp CreatePowerUp() => new HealthPowerUp(healAmount);
    }
}
