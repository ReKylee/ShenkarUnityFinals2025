using Collectables._Base;
using PowerUps._Base;

namespace PowerUps.FireFlower
{
    public class FireFlowerCollectible : PowerUpCollectibleBase
    {
        protected override IPowerUp CreatePowerUp() => new FireFlowerPowerUp();
    }
}
