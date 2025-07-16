using Health.Interfaces;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.OneUp
{
    public class OneUpPowerUp : IPowerUp
    {
        private readonly int _healAmount;
        public OneUpPowerUp(int healAmount = 1)
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
