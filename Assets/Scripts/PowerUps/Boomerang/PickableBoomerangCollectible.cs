using PowerUps._Base;

namespace PowerUps.Boomerang
{
    public class PickableBoomerangCollectible : PowerUpCollectibleBase
    {
        protected override IPowerUp CreatePowerUp() => new PickableBoomerangPowerUp();
    }
}
