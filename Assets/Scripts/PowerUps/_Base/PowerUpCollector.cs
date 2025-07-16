using UnityEngine;

namespace PowerUps._Base
{
    public class PowerUpCollector : MonoBehaviour, IPowerUpCollector
    {

        [SerializeField] public bool canCollectPowerUps = true;

        public bool CanCollectPowerUps
        {
            get => canCollectPowerUps;
            set => canCollectPowerUps = value;
        }

        public void ApplyPowerUp(IPowerUp powerUp)
        {
            if (CanCollectPowerUps)
            {
                powerUp.ApplyPowerUp(gameObject);
            }
        }
    }
}
