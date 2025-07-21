using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Invincibility
{
    public class InvincibilityPowerUp : IPowerUp
    {
        private readonly float _duration;

        public InvincibilityPowerUp(float duration)
        {
            _duration = duration;
        }

        public void ApplyPowerUp(GameObject player)
        {
            InvincibilityEffect handler = player.GetComponent<InvincibilityEffect>();
            handler?.Activate(_duration);
        }
    }
}
