using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Boomerang
{
    public class PickableBoomerangCollectible : PowerUpCollectibleBase
    {
        public override IPowerUp CreatePowerUp()
        {
            return new PickableBoomerangPowerUp();
        }
    }
}
