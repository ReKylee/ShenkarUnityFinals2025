using PowerUps._Base;

namespace PowerUps.LaserPowerUp
{
    public class LaserCollectible : PowerUpCollectibleBase
    {
        public override IPowerUp CreatePowerUp() => new LaserPowerUp();
    }
}
