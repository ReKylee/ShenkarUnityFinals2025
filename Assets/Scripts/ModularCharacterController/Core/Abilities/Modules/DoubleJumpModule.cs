using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Modules
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    /// </summary>
    public class DoubleJumpModule : AbilityModuleBase, IMovementAbilityModule
    {
        private bool _doubleJumped;
        public Vector2 ProcessMovement(
            Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext)
        {
            if (!Controller || !Controller.Stats) return currentVelocity;
            if (isGrounded)
            {
                _doubleJumped = false;
                return currentVelocity;
            }

            if (!_doubleJumped && inputContext.JumpPressed)
            {
                currentVelocity.y = Controller.Stats.flapImpulse;
                _doubleJumped = true;
            }

            return currentVelocity;
        }
    }
}
