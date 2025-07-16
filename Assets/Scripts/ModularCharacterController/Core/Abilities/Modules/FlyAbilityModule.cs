using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Modules
{
    /// <summary>
    ///     Fly ability - allows Kirby to fly by repeatedly tapping jump or holding it
    ///     When jump is released, Kirby slowly floats down
    /// </summary>
    public class FlyAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        private const float FLAP_COOLDOWN_TIME = 0.2f;

        private float _flapCooldown;
        private bool _isFlying;
        private bool _wasJumpHeld;

        public Vector2 ProcessMovement(Vector2 currentVelocity, bool isGrounded, InputContext inputContext)
        {
            // Don't reset flying state when grounded, only when attack is pressed
            // This allows flying to continue when touching ground
            if (isGrounded)
            {
                return currentVelocity;
            }

            // Stop flying immediately when attack is pressed
            if (inputContext.AttackPressed && _isFlying)
            {
                StopFlying();
                return currentVelocity;
            }


            UpdateFlapCooldown();

            // Start flying only when jump is pressed while in air
            if (inputContext.JumpPressed && !_isFlying)
            {
                StartFlying(ref currentVelocity);
                return currentVelocity;
            }

            // Only process flying mechanics if we're already in flying mode
            if (_isFlying)
            {
                return ProcessFlyingMovement(currentVelocity, inputContext);
            }

            return currentVelocity;
        }


        private void StopFlying()
        {
            _isFlying = false;
            _wasJumpHeld = false;
        }

        private void UpdateFlapCooldown()
        {
            if (_flapCooldown > 0)
            {
                _flapCooldown -= Time.deltaTime;
            }
        }

        private void StartFlying(ref Vector2 currentVelocity)
        {
            _isFlying = true;
            currentVelocity.y = Controller.Stats.flapImpulse;
            _flapCooldown = FLAP_COOLDOWN_TIME;
            _wasJumpHeld = true;
        }

        private Vector2 ProcessFlyingMovement(Vector2 currentVelocity, InputContext inputContext)
        {
            if (inputContext.JumpHeld)
            {
                return HandleJumpHeld(currentVelocity);
            }

            return HandleJumpReleased(currentVelocity);
        }

        private Vector2 HandleJumpHeld(Vector2 currentVelocity)
        {
            // Apply flap impulse when conditions are met
            if (ShouldApplyFlapImpulse())
            {
                currentVelocity.y = Controller.Stats.flapImpulse;
                _flapCooldown = FLAP_COOLDOWN_TIME;
            }

            _wasJumpHeld = true;
            return currentVelocity;
        }

        private bool ShouldApplyFlapImpulse() =>
            // Apply flap when jump wasn't held before (new press) or when cooldown is ready
            !_wasJumpHeld || _flapCooldown <= 0;

        private Vector2 HandleJumpReleased(Vector2 currentVelocity)
        {
            _wasJumpHeld = false;

            // Apply float descent when falling
            if (currentVelocity.y < 0)
            {
                currentVelocity.y *= Controller.Stats.floatDescentSpeed;
            }

            return currentVelocity;
        }
    }
}
