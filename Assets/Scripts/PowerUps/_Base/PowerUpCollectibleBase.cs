using Collectables._Base;
using UnityEngine;

namespace PowerUps._Base
{
    public abstract class PowerUpCollectibleBase : CollectibleBase
    {
        protected abstract IPowerUp CreatePowerUp();
        public override void OnCollect(GameObject collector)
        {
            Debug.Log("PowerUp collected: " + gameObject.name);

            IPowerUpCollector powerUpCollector = collector.GetComponent<IPowerUpCollector>();

            // NOTE: The Power Up still disappears even if you can't collect it.
            if (powerUpCollector?.CanCollectPowerUps != true)
                return;

            IPowerUp powerUp = CreatePowerUp();
            if (powerUp != null)
            {
                powerUpCollector.ApplyPowerUp(powerUp);
            }
        }
    }
}
