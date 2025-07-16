using ModularCharacterController.Core.Abilities;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Transformation
{
    public class TransformationPowerUp : IPowerUp
    {
        private readonly CopyAbilityData _abilityData;
        public TransformationPowerUp(CopyAbilityData abilityData)
        {
            _abilityData = abilityData;
        }
        public void ApplyPowerUp(GameObject player)
        {
            player.GetComponent<ModularCharacterController.Core.Components.ModularCharacterController>()
                .EquipAbility(_abilityData);
        }
    }
}
