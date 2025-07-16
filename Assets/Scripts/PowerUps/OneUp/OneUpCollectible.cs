using Collectables._Base;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.OneUp
{
    public class OneUpCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private int healAmount = 1;

        public override IPowerUp CreatePowerUp() => new OneUpPowerUp(healAmount);
    }
}
