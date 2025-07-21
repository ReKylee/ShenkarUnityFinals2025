using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Invincibility
{
    public class InvincibilityCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private float duration = 10f;

        protected override IPowerUp CreatePowerUp() => new InvincibilityPowerUp(duration);
    }
}
