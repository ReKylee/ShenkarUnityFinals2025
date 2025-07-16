using Collectables._Base;
using PowerUps._Base;

namespace PowerUps.Axe
{
    public class PickableAxeCollectible : PowerUpCollectibleBase
    {

        public override IPowerUp CreatePowerUp() => new PickableAxePowerUp();
    }
}
