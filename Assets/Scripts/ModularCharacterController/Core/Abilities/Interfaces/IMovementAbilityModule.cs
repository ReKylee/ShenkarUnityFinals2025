using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Interfaces
{
    /// <summary>
    ///     Interface for abilities that modify movement mechanics
    /// </summary>
    public interface IMovementAbilityModule : IAbilityModule
    {
        /// <summary>
        ///     Process movement modifications.
        /// </summary>
        /// <param name="currentVelocity">Current velocity before modification</param>
        /// <param name="isGrounded">Whether Kirby is on the ground</param>
        /// <param name="inputContext">The current input state.</param>
        /// <returns>Modified velocity</returns>
        Vector2 ProcessMovement(Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext);
    }
}
