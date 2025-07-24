using PowerUps._Base;

namespace PowerUps.Axe
{
    public class PickableAxeCollectible : PowerUpCollectibleBase
    {

        protected override IPowerUp CreatePowerUp() => new PickableAxePowerUp();
    }
}
