namespace PowerUps._Base
{
    public interface IPowerUpCollector
    {
        bool CanCollectPowerUps { get; }
        void ApplyPowerUp(IPowerUp powerUp);
    }
}
