using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Modules
{
    /// <summary>
    ///     Jump ability for Kirby - handles jumping mechanics including variable height jumps
    ///     Designed to mimic Kirby and the Amazing Mirror's GBA jump style
    /// </summary>
    public class JumpAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        private float _jumpStartTime;
        private float _lastGroundedTime;

        public Vector2 ProcessMovement(Vector2 currentVelocity, bool isGrounded, InputContext inputContext)
        {
            if (!Controller || !Controller.Stats) return currentVelocity;


            if (isGrounded)
            {
                _lastGroundedTime = Time.time;
            }

            if (inputContext.JumpPressed)
            {
                _jumpStartTime = Time.time;
            }

            // Check for jump execution
            bool coyoteTimeActive = Time.time - _lastGroundedTime <= Controller.Stats.coyoteTime;
            bool jumpBufferActive = Time.time - _jumpStartTime <= Controller.Stats.jumpBufferTime;

            if (coyoteTimeActive && jumpBufferActive)
            {
                currentVelocity.y = Controller.Stats.jumpVelocity;
                _lastGroundedTime = -100f;
                _jumpStartTime = -100f;
            }

            // Variable jump height: cut ascent when jump button is no longer held
            if (!inputContext.JumpHeld && currentVelocity.y > 0)
            {
                currentVelocity.y *= Controller.Stats.jumpReleaseVelocityMultiplier;
            }

            return currentVelocity;
        }

        public override void OnActivate()
        {
            // Initialize times to be far in the past, making them initially inactive
            _lastGroundedTime = -100f;
            _jumpStartTime = -100f;
        }

        public override void OnDeactivate()
        {
        }
    }
}
