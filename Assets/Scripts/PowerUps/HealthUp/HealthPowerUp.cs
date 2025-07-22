using Health.Interfaces;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.HealthUp
{
    public class HealthPowerUp : IPowerUp
    {
        private readonly int _healAmount;
        public HealthPowerUp(int healAmount = 1)
        {
            _healAmount = healAmount;
        }

        public void ApplyPowerUp(GameObject player)
        {
            IHealable damageable = player?.GetComponent<IHealable>();
            damageable?.Heal(_healAmount);
        }
    }
}
