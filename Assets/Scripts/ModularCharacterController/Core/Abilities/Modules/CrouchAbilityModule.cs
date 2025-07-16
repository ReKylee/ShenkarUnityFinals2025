using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Modules
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    /// </summary>
    public class CrouchAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        public Vector2 ProcessMovement(
            Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext)
        {
            if (!Controller || !Controller.Stats) return currentVelocity;

            if (inputContext.CrouchPressed)
            {
                currentVelocity.x = 0;
            }

            return currentVelocity;
        }
    }
}
