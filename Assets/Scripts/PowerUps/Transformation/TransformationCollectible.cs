using ModularCharacterController.Core.Abilities;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Transformation
{
    public class TransformationCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private CopyAbilityData abilityData;
        private void Start()
        {
            if (abilityData is { abilityIcon: { } icon })
            {
                GetComponent<SpriteRenderer>().sprite = icon;
            }
        }
        public override IPowerUp CreatePowerUp() => new TransformationPowerUp(abilityData);
    }
}
